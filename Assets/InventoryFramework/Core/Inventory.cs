using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InventoryFramework.Core
{
    /// <summary>
    /// Main inventory container class.
    /// Handles all inventory logic: adding, removing, finding, and managing slots.
    /// Works independently of UI - all UI connections happen through the EventBus.
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        [Header("Inventory Configuration")]
        [SerializeField] private int capacity = 20;
        [SerializeField] private bool useWeightLimit = false;
        [SerializeField] private float maxWeight = 100f;

        [Header("Slots")]
        [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();

        // Event bus for this inventory
        private InventoryEventBus eventBus;

        // Properties
        public int Capacity => capacity;
        public int UsedSlots => slots.Count(s => !s.IsEmpty);
        public int FreeSlots => capacity - UsedSlots;
        public bool IsFull => FreeSlots == 0;
        public IReadOnlyList<InventorySlot> Slots => slots;
        public InventoryEventBus EventBus => eventBus;

        // Weight tracking (optional feature)
        private float currentWeight = 0f;
        public float CurrentWeight => currentWeight;
        public float MaxWeight => maxWeight;
        public bool IsOverweight => useWeightLimit && currentWeight > maxWeight;

        private void Awake()
        {
            eventBus = new InventoryEventBus();
            InitializeSlots();
        }

        /// <summary>
        /// Initialize empty slots
        /// </summary>
        private void InitializeSlots()
        {
            slots.Clear();
            for (int i = 0; i < capacity; i++)
            {
                slots.Add(new InventorySlot());
            }
        }

        /// <summary>
        /// Add item to inventory
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="amount">Amount to add</param>
        /// <returns>Amount that couldn't be added (0 if all added successfully)</returns>
        public int AddItem(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0)
                return amount;

            int remaining = amount;

            // Try to stack with existing items first
            for (int i = 0; i < slots.Count && remaining > 0; i++)
            {
                if (!slots[i].IsEmpty && slots[i].Item == item && !slots[i].IsFull)
                {
                    int before = slots[i].Quantity;
                    remaining = slots[i].AddItem(item, remaining);
                    int added = slots[i].Quantity - before;

                    if (added > 0)
                    {
                        eventBus.RaiseItemAdded(item, added, i);
                        eventBus.RaiseSlotChanged(i, slots[i]);
                    }
                }
            }

            // Fill empty slots if needed
            for (int i = 0; i < slots.Count && remaining > 0; i++)
            {
                if (slots[i].IsEmpty)
                {
                    int before = 0;
                    remaining = slots[i].AddItem(item, remaining);
                    int added = slots[i].Quantity - before;

                    if (added > 0)
                    {
                        eventBus.RaiseItemAdded(item, added, i);
                        eventBus.RaiseSlotChanged(i, slots[i]);
                    }
                }
            }

            // If there's overflow, trigger full event
            if (remaining > 0)
            {
                eventBus.RaiseInventoryFull(item, remaining);
            }

            return remaining;
        }

        /// <summary>
        /// Remove item from inventory
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <param name="amount">Amount to remove</param>
        /// <returns>Amount actually removed</returns>
        public int RemoveItem(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0)
                return 0;

            int toRemove = amount;
            int totalRemoved = 0;

            // Remove from slots (starting from end to maintain order)
            for (int i = slots.Count - 1; i >= 0 && toRemove > 0; i--)
            {
                if (slots[i].Item == item)
                {
                    int removed = slots[i].RemoveItem(toRemove);
                    toRemove -= removed;
                    totalRemoved += removed;

                    if (removed > 0)
                    {
                        eventBus.RaiseItemRemoved(item, removed, i);
                        eventBus.RaiseSlotChanged(i, slots[i]);
                    }
                }
            }

            return totalRemoved;
        }

        /// <summary>
        /// Remove item from a specific slot
        /// </summary>
        public int RemoveItemFromSlot(int slotIndex, int amount)
        {
            if (!IsValidSlotIndex(slotIndex))
                return 0;

            var slot = slots[slotIndex];
            if (slot.IsEmpty)
                return 0;

            var item = slot.Item;
            int removed = slot.RemoveItem(amount);

            if (removed > 0)
            {
                eventBus.RaiseItemRemoved(item, removed, slotIndex);
                eventBus.RaiseSlotChanged(slotIndex, slot);
            }

            return removed;
        }

        /// <summary>
        /// Get item count in inventory
        /// </summary>
        public int GetItemCount(ItemDefinition item)
        {
            if (item == null)
                return 0;

            int count = 0;
            foreach (var slot in slots)
            {
                if (slot.Item == item)
                {
                    count += slot.Quantity;
                }
            }
            return count;
        }

        /// <summary>
        /// Check if inventory has enough of an item
        /// </summary>
        public bool HasItem(ItemDefinition item, int amount = 1)
        {
            return GetItemCount(item) >= amount;
        }

        /// <summary>
        /// Find first slot containing an item
        /// </summary>
        public int FindSlot(ItemDefinition item)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Item == item)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Find all slots containing an item
        /// </summary>
        public List<int> FindAllSlots(ItemDefinition item)
        {
            var indices = new List<int>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Item == item)
                    indices.Add(i);
            }
            return indices;
        }

        /// <summary>
        /// Get slot at index
        /// </summary>
        public InventorySlot GetSlot(int index)
        {
            if (!IsValidSlotIndex(index))
                return null;

            return slots[index];
        }

        /// <summary>
        /// Clear a specific slot
        /// </summary>
        public void ClearSlot(int index)
        {
            if (!IsValidSlotIndex(index))
                return;

            var slot = slots[index];
            if (!slot.IsEmpty)
            {
                var item = slot.Item;
                int quantity = slot.Quantity;
                slot.Clear();
                eventBus.RaiseItemRemoved(item, quantity, index);
                eventBus.RaiseSlotChanged(index, slot);
            }
        }

        /// <summary>
        /// Clear all slots
        /// </summary>
        public void ClearAll()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                ClearSlot(i);
            }
            eventBus.RaiseInventoryChanged();
        }

        /// <summary>
        /// Swap two slots
        /// </summary>
        public bool SwapSlots(int indexA, int indexB)
        {
            if (!IsValidSlotIndex(indexA) || !IsValidSlotIndex(indexB))
                return false;

            var temp = slots[indexA].Clone();
            var slotA = slots[indexA];
            var slotB = slots[indexB];

            // Perform swap
            slotA.Clear();
            slotA.AddItem(slotB.Item, slotB.Quantity);
            
            slotB.Clear();
            slotB.AddItem(temp.Item, temp.Quantity);

            // Copy metadata
            if (temp.RuntimeMetadata != null)
            {
                foreach (var kvp in temp.RuntimeMetadata)
                {
                    slotB.SetMetadata(kvp.Key, kvp.Value);
                }
            }

            eventBus.RaiseSlotChanged(indexA, slotA);
            eventBus.RaiseSlotChanged(indexB, slotB);

            return true;
        }

        /// <summary>
        /// Check if slot index is valid
        /// </summary>
        private bool IsValidSlotIndex(int index)
        {
            return index >= 0 && index < slots.Count;
        }

        /// <summary>
        /// Get items by category
        /// </summary>
        public List<InventorySlot> GetItemsByCategory(string category)
        {
            return slots.Where(s => !s.IsEmpty && s.Item.IsCategory(category)).ToList();
        }

        /// <summary>
        /// Get items by tag
        /// </summary>
        public List<InventorySlot> GetItemsByTag(string tag)
        {
            return slots.Where(s => !s.IsEmpty && s.Item.HasTag(tag)).ToList();
        }

        /// <summary>
        /// Resize inventory capacity
        /// </summary>
        public void SetCapacity(int newCapacity)
        {
            if (newCapacity < 0)
                return;

            int oldCapacity = capacity;
            capacity = newCapacity;

            // Add new slots if capacity increased
            if (newCapacity > oldCapacity)
            {
                for (int i = oldCapacity; i < newCapacity; i++)
                {
                    slots.Add(new InventorySlot());
                }
            }
            // Remove slots if capacity decreased (clear items first)
            else if (newCapacity < oldCapacity)
            {
                for (int i = oldCapacity - 1; i >= newCapacity; i--)
                {
                    if (i < slots.Count)
                    {
                        ClearSlot(i);
                        slots.RemoveAt(i);
                    }
                }
            }

            eventBus.RaiseInventoryChanged();
        }

        private void OnDestroy()
        {
            // Clean up event subscriptions
            eventBus?.ClearAllSubscriptions();
        }
    }
}

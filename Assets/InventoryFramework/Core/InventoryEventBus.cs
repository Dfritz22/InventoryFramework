using System;
using UnityEngine;

namespace InventoryFramework.Core
{
    /// <summary>
    /// Event system for inventory changes.
    /// UI and gameplay systems subscribe to these events to react to inventory changes.
    /// This is what makes the framework UI-agnostic.
    /// </summary>
    public class InventoryEventBus
    {
        // Event delegates
        public event Action<ItemDefinition, int, int> OnItemAdded;
        public event Action<ItemDefinition, int, int> OnItemRemoved;
        public event Action<int, ItemDefinition, int> OnStackChanged;
        public event Action<int, InventorySlot> OnSlotChanged;
        public event Action OnInventoryChanged;
        public event Action<ItemDefinition, int> OnInventoryFull;

        /// <summary>
        /// Raise when an item is added to the inventory
        /// </summary>
        /// <param name="item">Item that was added</param>
        /// <param name="amount">Amount added</param>
        /// <param name="slotIndex">Slot index where it was added</param>
        public void RaiseItemAdded(ItemDefinition item, int amount, int slotIndex)
        {
            OnItemAdded?.Invoke(item, amount, slotIndex);
            RaiseInventoryChanged();
        }

        /// <summary>
        /// Raise when an item is removed from the inventory
        /// </summary>
        /// <param name="item">Item that was removed</param>
        /// <param name="amount">Amount removed</param>
        /// <param name="slotIndex">Slot index from where it was removed</param>
        public void RaiseItemRemoved(ItemDefinition item, int amount, int slotIndex)
        {
            OnItemRemoved?.Invoke(item, amount, slotIndex);
            RaiseInventoryChanged();
        }

        /// <summary>
        /// Raise when a stack size changes in a slot
        /// </summary>
        /// <param name="slotIndex">Slot that changed</param>
        /// <param name="item">Item in the slot</param>
        /// <param name="newQuantity">New quantity</param>
        public void RaiseStackChanged(int slotIndex, ItemDefinition item, int newQuantity)
        {
            OnStackChanged?.Invoke(slotIndex, item, newQuantity);
            RaiseInventoryChanged();
        }

        /// <summary>
        /// Raise when a slot changes (item or quantity)
        /// </summary>
        /// <param name="slotIndex">Slot that changed</param>
        /// <param name="slot">The slot data</param>
        public void RaiseSlotChanged(int slotIndex, InventorySlot slot)
        {
            OnSlotChanged?.Invoke(slotIndex, slot);
            RaiseInventoryChanged();
        }

        /// <summary>
        /// Raise when inventory changes in any way
        /// </summary>
        public void RaiseInventoryChanged()
        {
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Raise when trying to add an item but inventory is full
        /// </summary>
        /// <param name="item">Item that couldn't be added</param>
        /// <param name="overflow">Amount that couldn't fit</param>
        public void RaiseInventoryFull(ItemDefinition item, int overflow)
        {
            OnInventoryFull?.Invoke(item, overflow);
        }

        /// <summary>
        /// Clear all event subscriptions
        /// Useful for cleanup
        /// </summary>
        public void ClearAllSubscriptions()
        {
            OnItemAdded = null;
            OnItemRemoved = null;
            OnStackChanged = null;
            OnSlotChanged = null;
            OnInventoryChanged = null;
            OnInventoryFull = null;
        }
    }
}

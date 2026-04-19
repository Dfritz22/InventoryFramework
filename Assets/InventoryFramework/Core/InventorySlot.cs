using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryFramework.Core
{
    /// <summary>
    /// Represents a single slot in an inventory.
    /// Handles item storage, quantity, and runtime metadata.
    /// </summary>
    [Serializable]
    public class InventorySlot
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private int quantity;
        
        // Runtime metadata - not serialized by default, handled by InventorySerializer
        private Dictionary<string, string> runtimeMetadata;

        public ItemDefinition Item => item;
        public int Quantity => quantity;
        public bool IsEmpty => item == null || quantity <= 0;
        public bool IsFull => item != null && quantity >= item.MaxStackSize;

        /// <summary>
        /// Get or create runtime metadata dictionary
        /// </summary>
        public Dictionary<string, string> RuntimeMetadata
        {
            get
            {
                if (runtimeMetadata == null)
                {
                    runtimeMetadata = new Dictionary<string, string>();
                }
                return runtimeMetadata;
            }
        }

        // Constructors
        public InventorySlot()
        {
            item = null;
            quantity = 0;
        }

        public InventorySlot(ItemDefinition itemDef, int qty)
        {
            item = itemDef;
            quantity = Mathf.Max(0, qty);
        }

        /// <summary>
        /// Try to add items to this slot
        /// </summary>
        /// <param name="itemDef">Item to add</param>
        /// <param name="amount">Amount to add</param>
        /// <returns>Amount that couldn't be added (overflow)</returns>
        public int AddItem(ItemDefinition itemDef, int amount)
        {
            if (itemDef == null || amount <= 0)
                return amount;

            // If slot is empty, just add the item
            if (IsEmpty)
            {
                item = itemDef;
                int toAdd = Mathf.Min(amount, itemDef.MaxStackSize);
                quantity = toAdd;
                return amount - toAdd;
            }

            // If different item, can't stack
            if (item != itemDef)
                return amount;

            // Calculate how much can be added
            int space = item.MaxStackSize - quantity;
            int toAdd = Mathf.Min(amount, space);
            
            quantity += toAdd;
            return amount - toAdd;
        }

        /// <summary>
        /// Remove items from this slot
        /// </summary>
        /// <param name="amount">Amount to remove</param>
        /// <returns>Amount actually removed</returns>
        public int RemoveItem(int amount)
        {
            if (IsEmpty || amount <= 0)
                return 0;

            int toRemove = Mathf.Min(amount, quantity);
            quantity -= toRemove;

            // Clear slot if empty
            if (quantity <= 0)
            {
                Clear();
            }

            return toRemove;
        }

        /// <summary>
        /// Check if this slot can accept the given item
        /// </summary>
        public bool CanAcceptItem(ItemDefinition itemDef, int amount)
        {
            if (itemDef == null || amount <= 0)
                return false;

            if (IsEmpty)
                return true;

            if (item != itemDef)
                return false;

            int space = item.MaxStackSize - quantity;
            return space > 0;
        }

        /// <summary>
        /// Get remaining stack space
        /// </summary>
        public int GetAvailableSpace()
        {
            if (IsEmpty)
                return int.MaxValue; // Empty slots can hold anything

            return item.MaxStackSize - quantity;
        }

        /// <summary>
        /// Clear the slot completely
        /// </summary>
        public void Clear()
        {
            item = null;
            quantity = 0;
            runtimeMetadata?.Clear();
        }

        /// <summary>
        /// Set runtime metadata value
        /// </summary>
        public void SetMetadata(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            RuntimeMetadata[key] = value;
        }

        /// <summary>
        /// Get runtime metadata value
        /// </summary>
        public string GetMetadata(string key, string defaultValue = "")
        {
            if (runtimeMetadata == null || string.IsNullOrEmpty(key))
                return defaultValue;

            return runtimeMetadata.TryGetValue(key, out string value) ? value : defaultValue;
        }

        /// <summary>
        /// Check if metadata exists
        /// </summary>
        public bool HasMetadata(string key)
        {
            return runtimeMetadata != null && runtimeMetadata.ContainsKey(key);
        }

        /// <summary>
        /// Clone this slot
        /// </summary>
        public InventorySlot Clone()
        {
            var clone = new InventorySlot(item, quantity);
            
            if (runtimeMetadata != null)
            {
                foreach (var kvp in runtimeMetadata)
                {
                    clone.SetMetadata(kvp.Key, kvp.Value);
                }
            }

            return clone;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryFramework.Core
{
    /// <summary>
    /// Handles serialization and deserialization of inventory data.
    /// Supports JSON format with versioning for future compatibility.
    /// </summary>
    public static class InventorySerializer
    {
        private const int CURRENT_VERSION = 1;

        /// <summary>
        /// Serialize an inventory to JSON string
        /// </summary>
        public static string SerializeToJson(Inventory inventory)
        {
            if (inventory == null)
                return string.Empty;

            var data = new InventoryData
            {
                version = CURRENT_VERSION,
                capacity = inventory.Capacity,
                slots = new List<SlotData>()
            };

            for (int i = 0; i < inventory.Slots.Count; i++)
            {
                var slot = inventory.Slots[i];
                if (!slot.IsEmpty)
                {
                    var slotData = new SlotData
                    {
                        slotIndex = i,
                        itemName = slot.Item.name, // ScriptableObject asset name
                        quantity = slot.Quantity,
                        metadata = new List<MetadataEntry>()
                    };

                    // Serialize runtime metadata
                    if (slot.RuntimeMetadata != null)
                    {
                        foreach (var kvp in slot.RuntimeMetadata)
                        {
                            slotData.metadata.Add(new MetadataEntry
                            {
                                key = kvp.Key,
                                value = kvp.Value
                            });
                        }
                    }

                    data.slots.Add(slotData);
                }
            }

            return JsonUtility.ToJson(data, true);
        }

        /// <summary>
        /// Deserialize JSON string to populate an inventory
        /// </summary>
        public static bool DeserializeFromJson(string json, Inventory inventory)
        {
            if (string.IsNullOrEmpty(json) || inventory == null)
                return false;

            try
            {
                var data = JsonUtility.FromJson<InventoryData>(json);

                if (data == null)
                    return false;

                // Handle version compatibility
                if (data.version > CURRENT_VERSION)
                {
                    Debug.LogWarning($"Inventory data version {data.version} is newer than current version {CURRENT_VERSION}. Some data may be lost.");
                }

                // Clear existing inventory
                inventory.ClearAll();

                // Adjust capacity if needed
                if (data.capacity != inventory.Capacity)
                {
                    inventory.SetCapacity(data.capacity);
                }

                // Restore slots
                foreach (var slotData in data.slots)
                {
                    // Load item by name (requires item to be in Resources or AssetDatabase)
                    var item = LoadItemByName(slotData.itemName);
                    
                    if (item != null && slotData.slotIndex >= 0 && slotData.slotIndex < inventory.Capacity)
                    {
                        inventory.GetSlot(slotData.slotIndex).AddItem(item, slotData.quantity);

                        // Restore metadata
                        if (slotData.metadata != null)
                        {
                            foreach (var meta in slotData.metadata)
                            {
                                inventory.GetSlot(slotData.slotIndex).SetMetadata(meta.key, meta.value);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Could not restore item '{slotData.itemName}' at slot {slotData.slotIndex}");
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize inventory: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Save inventory to PlayerPrefs
        /// </summary>
        public static void SaveToPlayerPrefs(Inventory inventory, string key = "InventoryData")
        {
            string json = SerializeToJson(inventory);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load inventory from PlayerPrefs
        /// </summary>
        public static bool LoadFromPlayerPrefs(Inventory inventory, string key = "InventoryData")
        {
            if (!PlayerPrefs.HasKey(key))
                return false;

            string json = PlayerPrefs.GetString(key);
            return DeserializeFromJson(json, inventory);
        }

        /// <summary>
        /// Save inventory to file
        /// </summary>
        public static void SaveToFile(Inventory inventory, string filePath)
        {
            try
            {
                string json = SerializeToJson(inventory);
                System.IO.File.WriteAllText(filePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save inventory to file: {e.Message}");
            }
        }

        /// <summary>
        /// Load inventory from file
        /// </summary>
        public static bool LoadFromFile(Inventory inventory, string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                    return false;

                string json = System.IO.File.ReadAllText(filePath);
                return DeserializeFromJson(json, inventory);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load inventory from file: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load ItemDefinition by name
        /// Override this method or use a custom item database for production
        /// </summary>
        private static ItemDefinition LoadItemByName(string itemName)
        {
            // Try to load from Resources folder
            // In production, you'd want to use a custom ItemDatabase or AssetDatabase
            var item = Resources.Load<ItemDefinition>(itemName);
            
            if (item == null)
            {
                // Try with full path
                item = Resources.Load<ItemDefinition>($"Items/{itemName}");
            }

            return item;
        }

        // Serializable data structures
        [Serializable]
        private class InventoryData
        {
            public int version;
            public int capacity;
            public List<SlotData> slots;
        }

        [Serializable]
        private class SlotData
        {
            public int slotIndex;
            public string itemName;
            public int quantity;
            public List<MetadataEntry> metadata;
        }

        [Serializable]
        private class MetadataEntry
        {
            public string key;
            public string value;
        }
    }
}

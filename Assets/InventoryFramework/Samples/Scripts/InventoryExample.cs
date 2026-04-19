using UnityEngine;
using InventoryFramework.Core;

namespace InventoryFramework.Samples
{
    /// <summary>
    /// Example script demonstrating how to use the Inventory Framework.
    /// This shows the basic integration - developers can adapt this to their own UI and gameplay.
    /// </summary>
    public class InventoryExample : MonoBehaviour
    {
        [Header("Inventory Reference")]
        [SerializeField] private Inventory playerInventory;

        [Header("Test Items")]
        [SerializeField] private ItemDefinition testItem1;
        [SerializeField] private ItemDefinition testItem2;

        private void Start()
        {
            // Subscribe to inventory events
            // This is how you connect the framework to your UI
            if (playerInventory != null)
            {
                SetupEventListeners();
            }
        }

        private void SetupEventListeners()
        {
            // These events let your UI react to inventory changes
            playerInventory.EventBus.OnItemAdded += HandleItemAdded;
            playerInventory.EventBus.OnItemRemoved += HandleItemRemoved;
            playerInventory.EventBus.OnSlotChanged += HandleSlotChanged;
            playerInventory.EventBus.OnInventoryFull += HandleInventoryFull;
            playerInventory.EventBus.OnInventoryChanged += HandleInventoryChanged;
        }

        #region Event Handlers (Connect these to your UI)

        private void HandleItemAdded(ItemDefinition item, int amount, int slotIndex)
        {
            Debug.Log($"Item Added: {item.ItemName} x{amount} to slot {slotIndex}");
            // Update your UI here - e.g., refresh slot visuals, play sound, show notification
        }

        private void HandleItemRemoved(ItemDefinition item, int amount, int slotIndex)
        {
            Debug.Log($"Item Removed: {item.ItemName} x{amount} from slot {slotIndex}");
            // Update your UI here
        }

        private void HandleSlotChanged(int slotIndex, InventorySlot slot)
        {
            Debug.Log($"Slot {slotIndex} changed: {(slot.IsEmpty ? "Empty" : $"{slot.Item.ItemName} x{slot.Quantity}")}");
            // Update specific slot UI element here
        }

        private void HandleInventoryFull(ItemDefinition item, int overflow)
        {
            Debug.LogWarning($"Inventory Full! Could not add {overflow} of {item.ItemName}");
            // Show "inventory full" message to player
        }

        private void HandleInventoryChanged()
        {
            // General inventory update - use for full refreshes
            Debug.Log("Inventory changed");
        }

        #endregion

        #region Example Usage (Remove these in production - just for testing)

        private void Update()
        {
            if (playerInventory == null)
                return;

            // Example: Press '1' to add test item
            if (Input.GetKeyDown(KeyCode.Alpha1) && testItem1 != null)
            {
                int remaining = playerInventory.AddItem(testItem1, 1);
                if (remaining > 0)
                {
                    Debug.Log("Couldn't add all items - inventory may be full");
                }
            }

            // Example: Press '2' to add 5 of test item 2
            if (Input.GetKeyDown(KeyCode.Alpha2) && testItem2 != null)
            {
                playerInventory.AddItem(testItem2, 5);
            }

            // Example: Press 'R' to remove item
            if (Input.GetKeyDown(KeyCode.R) && testItem1 != null)
            {
                int removed = playerInventory.RemoveItem(testItem1, 1);
                Debug.Log($"Removed {removed} items");
            }

            // Example: Press 'C' to clear inventory
            if (Input.GetKeyDown(KeyCode.C))
            {
                playerInventory.ClearAll();
            }

            // Example: Press 'S' to save inventory
            if (Input.GetKeyDown(KeyCode.S))
            {
                InventorySerializer.SaveToPlayerPrefs(playerInventory);
                Debug.Log("Inventory saved!");
            }

            // Example: Press 'L' to load inventory
            if (Input.GetKeyDown(KeyCode.L))
            {
                bool loaded = InventorySerializer.LoadFromPlayerPrefs(playerInventory);
                Debug.Log(loaded ? "Inventory loaded!" : "No save data found");
            }

            // Example: Press 'I' to print inventory contents
            if (Input.GetKeyDown(KeyCode.I))
            {
                PrintInventoryContents();
            }
        }

        private void PrintInventoryContents()
        {
            Debug.Log("=== Inventory Contents ===");
            Debug.Log($"Capacity: {playerInventory.Capacity}");
            Debug.Log($"Used Slots: {playerInventory.UsedSlots}/{playerInventory.Capacity}");
            
            for (int i = 0; i < playerInventory.Slots.Count; i++)
            {
                var slot = playerInventory.Slots[i];
                if (!slot.IsEmpty)
                {
                    Debug.Log($"Slot {i}: {slot.Item.ItemName} x{slot.Quantity}");
                }
            }
        }

        #endregion

        #region Advanced Examples

        /// <summary>
        /// Example: How to query inventory
        /// </summary>
        private void ExampleQueries()
        {
            // Check if player has an item
            if (playerInventory.HasItem(testItem1, 5))
            {
                Debug.Log("Player has at least 5 of item 1");
            }

            // Get total count of an item
            int count = playerInventory.GetItemCount(testItem1);
            Debug.Log($"Player has {count} of item 1");

            // Find slot containing an item
            int slotIndex = playerInventory.FindSlot(testItem1);
            if (slotIndex >= 0)
            {
                Debug.Log($"Item 1 is in slot {slotIndex}");
            }

            // Get items by category
            var weapons = playerInventory.GetItemsByCategory("Weapon");
            Debug.Log($"Found {weapons.Count} weapons");

            // Get items by tag
            var consumables = playerInventory.GetItemsByTag("Consumable");
            Debug.Log($"Found {consumables.Count} consumables");
        }

        /// <summary>
        /// Example: Working with metadata
        /// </summary>
        private void ExampleMetadata()
        {
            // Get first slot with our test item
            int slotIndex = playerInventory.FindSlot(testItem1);
            if (slotIndex >= 0)
            {
                var slot = playerInventory.GetSlot(slotIndex);

                // Set custom metadata (e.g., durability, ammo, quality)
                slot.SetMetadata("durability", "75");
                slot.SetMetadata("enchantment", "fire");

                // Read metadata
                string durability = slot.GetMetadata("durability", "100");
                Debug.Log($"Item durability: {durability}");

                // Check if metadata exists
                if (slot.HasMetadata("enchantment"))
                {
                    string enchant = slot.GetMetadata("enchantment");
                    Debug.Log($"Enchantment: {enchant}");
                }
            }
        }

        /// <summary>
        /// Example: Swapping slots (for drag-and-drop UI)
        /// </summary>
        private void ExampleSlotSwap()
        {
            // Swap slots 0 and 5
            playerInventory.SwapSlots(0, 5);
        }

        #endregion

        private void OnDestroy()
        {
            // Clean up event subscriptions
            if (playerInventory != null && playerInventory.EventBus != null)
            {
                playerInventory.EventBus.OnItemAdded -= HandleItemAdded;
                playerInventory.EventBus.OnItemRemoved -= HandleItemRemoved;
                playerInventory.EventBus.OnSlotChanged -= HandleSlotChanged;
                playerInventory.EventBus.OnInventoryFull -= HandleInventoryFull;
                playerInventory.EventBus.OnInventoryChanged -= HandleInventoryChanged;
            }
        }
    }
}

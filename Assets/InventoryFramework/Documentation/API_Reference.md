# Inventory Framework - API Reference

Complete API documentation for Milestone 1 - Core Engine.

---

## ItemDefinition (ScriptableObject)

Defines item data. Create via `Right-click > Create > Inventory Framework > Item Definition`

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ItemName` | string | Display name of the item |
| `Description` | string | Item description |
| `Icon` | Sprite | Item icon (optional) |
| `MaxStackSize` | int | Maximum stack size (min: 1) |
| `Category` | string | Item category for filtering |
| `Tags` | IReadOnlyList<string> | List of tags |

### Methods

```csharp
Dictionary<string, string> GetBaseMetadata()
```
Returns base metadata as dictionary.

```csharp
bool HasTag(string tag)
```
Checks if item has specific tag.

```csharp
bool IsCategory(string category)
```
Checks if item belongs to category.

---

## InventorySlot

Represents a single inventory slot.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Item` | ItemDefinition | Item in this slot (null if empty) |
| `Quantity` | int | Current quantity |
| `IsEmpty` | bool | True if slot is empty |
| `IsFull` | bool | True if stack is at max |
| `RuntimeMetadata` | Dictionary<string, string> | Runtime metadata dictionary |

### Methods

```csharp
int AddItem(ItemDefinition item, int amount)
```
Add items to slot. Returns overflow amount.

```csharp
int RemoveItem(int amount)
```
Remove items from slot. Returns amount actually removed.

```csharp
bool CanAcceptItem(ItemDefinition item, int amount)
```
Check if slot can accept item.

```csharp
int GetAvailableSpace()
```
Get remaining stack space.

```csharp
void Clear()
```
Clear the slot completely.

```csharp
void SetMetadata(string key, string value)
```
Set runtime metadata value.

```csharp
string GetMetadata(string key, string defaultValue = "")
```
Get runtime metadata value.

```csharp
bool HasMetadata(string key)
```
Check if metadata key exists.

```csharp
InventorySlot Clone()
```
Create a copy of this slot.

---

## Inventory (MonoBehaviour)

Main inventory container.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Capacity` | int | Total number of slots |
| `UsedSlots` | int | Number of non-empty slots |
| `FreeSlots` | int | Number of empty slots |
| `IsFull` | bool | True if no free slots |
| `Slots` | IReadOnlyList<InventorySlot> | All slots (read-only) |
| `EventBus` | InventoryEventBus | Event system for this inventory |
| `CurrentWeight` | float | Current total weight (if enabled) |
| `MaxWeight` | float | Maximum weight limit |
| `IsOverweight` | bool | True if over weight limit |

### Methods

#### Adding/Removing

```csharp
int AddItem(ItemDefinition item, int amount)
```
Add items to inventory. Returns overflow (0 if all added).

**Example:**
```csharp
int overflow = inventory.AddItem(woodItem, 10);
if (overflow > 0)
    Debug.Log($"Couldn't add {overflow} items");
```

```csharp
int RemoveItem(ItemDefinition item, int amount)
```
Remove items from inventory. Returns amount actually removed.

```csharp
int RemoveItemFromSlot(int slotIndex, int amount)
```
Remove items from specific slot.

#### Querying

```csharp
int GetItemCount(ItemDefinition item)
```
Get total count of item in inventory.

```csharp
bool HasItem(ItemDefinition item, int amount = 1)
```
Check if inventory contains enough of an item.

```csharp
int FindSlot(ItemDefinition item)
```
Find first slot containing item. Returns -1 if not found.

```csharp
List<int> FindAllSlots(ItemDefinition item)
```
Find all slots containing item.

```csharp
List<InventorySlot> GetItemsByCategory(string category)
```
Get all items in a category.

```csharp
List<InventorySlot> GetItemsByTag(string tag)
```
Get all items with a tag.

#### Slot Management

```csharp
InventorySlot GetSlot(int index)
```
Get slot at index.

```csharp
void ClearSlot(int index)
```
Clear specific slot.

```csharp
void ClearAll()
```
Clear all slots.

```csharp
bool SwapSlots(int indexA, int indexB)
```
Swap two slots (useful for drag & drop).

```csharp
void SetCapacity(int newCapacity)
```
Change inventory capacity.

---

## InventoryEventBus

Event system for inventory changes.

### Events

```csharp
event Action<ItemDefinition, int, int> OnItemAdded
```
Triggered when item is added. Parameters: (item, amount, slotIndex)

```csharp
event Action<ItemDefinition, int, int> OnItemRemoved
```
Triggered when item is removed. Parameters: (item, amount, slotIndex)

```csharp
event Action<int, ItemDefinition, int> OnStackChanged
```
Triggered when stack size changes. Parameters: (slotIndex, item, newQuantity)

```csharp
event Action<int, InventorySlot> OnSlotChanged
```
Triggered when slot changes. Parameters: (slotIndex, slot)

```csharp
event Action OnInventoryChanged
```
Triggered on any inventory change.

```csharp
event Action<ItemDefinition, int> OnInventoryFull
```
Triggered when trying to add item to full inventory. Parameters: (item, overflow)

### Example Usage

```csharp
void Start()
{
    inventory.EventBus.OnItemAdded += OnItemAdded;
    inventory.EventBus.OnSlotChanged += OnSlotChanged;
}

void OnItemAdded(ItemDefinition item, int amount, int slot)
{
    Debug.Log($"Added {amount}x {item.ItemName} to slot {slot}");
    UpdateUI();
}

void OnSlotChanged(int slotIndex, InventorySlot slot)
{
    myUISlots[slotIndex].Refresh(slot);
}

void OnDestroy()
{
    inventory.EventBus.OnItemAdded -= OnItemAdded;
    inventory.EventBus.OnSlotChanged -= OnSlotChanged;
}
```

---

## InventorySerializer

Static class for saving/loading inventory data.

### Methods

```csharp
static string SerializeToJson(Inventory inventory)
```
Convert inventory to JSON string.

```csharp
static bool DeserializeFromJson(string json, Inventory inventory)
```
Load inventory from JSON string.

```csharp
static void SaveToPlayerPrefs(Inventory inventory, string key = "InventoryData")
```
Save inventory to PlayerPrefs.

```csharp
static bool LoadFromPlayerPrefs(Inventory inventory, string key = "InventoryData")
```
Load inventory from PlayerPrefs.

```csharp
static void SaveToFile(Inventory inventory, string filePath)
```
Save inventory to JSON file.

```csharp
static bool LoadFromFile(Inventory inventory, string filePath)
```
Load inventory from JSON file.

### Example Usage

```csharp
// Save
InventorySerializer.SaveToPlayerPrefs(playerInventory);

// Load
if (InventorySerializer.LoadFromPlayerPrefs(playerInventory))
{
    Debug.Log("Inventory loaded successfully");
}
else
{
    Debug.Log("No save data found");
}

// File-based saving
string savePath = Application.persistentDataPath + "/inventory.json";
InventorySerializer.SaveToFile(playerInventory, savePath);
InventorySerializer.LoadFromFile(playerInventory, savePath);
```

### Notes

- Items are loaded by name from Resources folder
- For production, implement custom item database
- Supports versioning for future compatibility
- Runtime metadata is fully serialized

---

## Complete Integration Example

```csharp
using UnityEngine;
using InventoryFramework.Core;

public class MyInventoryManager : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemDefinition healthPotion;

    void Start()
    {
        // Subscribe to events
        inventory.EventBus.OnItemAdded += HandleItemAdded;
        inventory.EventBus.OnInventoryFull += HandleInventoryFull;
        
        // Load saved inventory
        InventorySerializer.LoadFromPlayerPrefs(inventory);
    }

    public void PickupItem(ItemDefinition item, int amount)
    {
        int overflow = inventory.AddItem(item, amount);
        if (overflow > 0)
        {
            // Drop items that didn't fit
            DropItems(item, overflow);
        }
    }

    public void UseHealthPotion()
    {
        if (inventory.HasItem(healthPotion))
        {
            inventory.RemoveItem(healthPotion, 1);
            // Heal player
            PlayerHealth.Heal(50);
        }
    }

    void HandleItemAdded(ItemDefinition item, int amount, int slot)
    {
        Debug.Log($"Picked up {amount}x {item.ItemName}");
        // Update UI, play sound, show notification
    }

    void HandleInventoryFull(ItemDefinition item, int overflow)
    {
        ShowMessage("Inventory is full!");
    }

    void OnApplicationQuit()
    {
        // Auto-save on quit
        InventorySerializer.SaveToPlayerPrefs(inventory);
    }

    void OnDestroy()
    {
        // Cleanup
        inventory.EventBus.OnItemAdded -= HandleItemAdded;
        inventory.EventBus.OnInventoryFull -= HandleInventoryFull;
    }
}
```

---

## Namespace

All framework classes are in the `InventoryFramework.Core` namespace.

```csharp
using InventoryFramework.Core;
```

---

*This API reference covers Milestone 1. Future milestones will add Rule Blocks, Metadata Channels, Slot Types, and UI Binding Wizard.*

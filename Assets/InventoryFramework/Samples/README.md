# Inventory Framework - Sample Usage

This folder contains sample scripts and items demonstrating how to use the Inventory Framework in your Unity project.

## Quick Start

### 1. Create an Inventory

1. Create an empty GameObject in your scene
2. Add the `Inventory` component to it
3. Set the capacity (number of slots)
4. (Optional) Enable weight limits if needed

### 2. Create Items

1. Right-click in Project window
2. Select `Create > Inventory Framework > Item Definition`
3. Configure the item properties:
   - Name, description, icon
   - Max stack size
   - Category and tags
   - Custom metadata

### 3. Use the Example Script

1. Add `InventoryExample.cs` to a GameObject
2. Assign your Inventory component
3. Assign test items
4. Press Play and use keyboard shortcuts:
   - `1` - Add test item 1
   - `2` - Add 5 of test item 2
   - `R` - Remove item
   - `C` - Clear inventory
   - `S` - Save to PlayerPrefs
   - `L` - Load from PlayerPrefs
   - `I` - Print inventory contents

## Integration with Your UI

The framework is **UI-agnostic** - it works with any UI system. Here's how to connect it:

### Event-Driven Approach

```csharp
// Subscribe to inventory events
inventory.EventBus.OnItemAdded += (item, amount, slot) => {
    // Update your UI - refresh slot display, play sound, etc.
    UpdateSlotUI(slot);
};

inventory.EventBus.OnSlotChanged += (slotIndex, slot) => {
    // Update specific slot
    myUISlots[slotIndex].Refresh(slot);
};

inventory.EventBus.OnInventoryFull += (item, overflow) => {
    // Show "inventory full" message
    ShowNotification("Inventory is full!");
};
```

### Direct Access Approach

```csharp
// You can also directly access inventory data
for (int i = 0; i < inventory.Slots.Count; i++)
{
    var slot = inventory.Slots[i];
    if (!slot.IsEmpty)
    {
        myUISlots[i].SetItem(slot.Item.Icon, slot.Quantity);
    }
}
```

## Common Operations

### Adding Items
```csharp
int overflow = inventory.AddItem(itemDefinition, 10);
if (overflow > 0)
{
    // Couldn't add all items
}
```

### Removing Items
```csharp
int removed = inventory.RemoveItem(itemDefinition, 5);
// Returns actual amount removed
```

### Checking Inventory
```csharp
if (inventory.HasItem(woodItem, 10))
{
    // Player has at least 10 wood
}

int totalWood = inventory.GetItemCount(woodItem);
```

### Working with Metadata
```csharp
var slot = inventory.GetSlot(slotIndex);

// Set custom data
slot.SetMetadata("durability", "85");
slot.SetMetadata("level", "5");

// Read custom data
string durability = slot.GetMetadata("durability", "100");
```

### Saving/Loading
```csharp
// Save to PlayerPrefs
InventorySerializer.SaveToPlayerPrefs(inventory);

// Load from PlayerPrefs
InventorySerializer.LoadFromPlayerPrefs(inventory);

// Save to file
InventorySerializer.SaveToFile(inventory, "path/to/save.json");

// Load from file
InventorySerializer.LoadFromFile(inventory, "path/to/save.json");
```

## Best Practices

1. **Subscribe to Events Early** - Set up your event listeners in `Start()` or `Awake()`
2. **Unsubscribe on Destroy** - Always clean up event subscriptions in `OnDestroy()`
3. **Use Events for UI** - Let the EventBus drive your UI updates instead of polling
4. **Keep Items in Resources** - If using serialization, keep ItemDefinitions in a `Resources/Items/` folder
5. **Don't Store References** - Store item counts/IDs, not direct references to inventory slots

## Advanced Features

### Query by Category/Tag
```csharp
var weapons = inventory.GetItemsByCategory("Weapon");
var consumables = inventory.GetItemsByTag("Consumable");
```

### Swap Slots (Drag & Drop)
```csharp
inventory.SwapSlots(sourceSlotIndex, targetSlotIndex);
```

### Dynamic Capacity
```csharp
inventory.SetCapacity(40); // Expand to 40 slots
```

## What's Next?

This is **Milestone 1** - the core engine. Future updates will add:
- **Rule Blocks** - Custom behavior (spoilage, weight, restrictions)
- **Metadata Channels** - Typed metadata system
- **Slot Types** - Equipment slots, hotbar, restricted slots
- **UI Binding Wizard** - Auto-connect UI elements

## Need Help?

Check the Documentation folder for architecture details and API reference.

---

**Remember:** This framework is UI-agnostic by design. You bring the visuals, we handle the logic!

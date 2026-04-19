# Unity Inventory Framework

**A complete, modular, UI-agnostic inventory system for Unity**

[![Unity](https://img.shields.io/badge/Unity-2020.3%2B-black.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

---

## 🎯 Overview

The Unity Inventory Framework is a professional, asset store-quality inventory system designed to work with **any** Unity project. It's:

- ✅ **UI-Agnostic** - Works with any UI system (uGUI, UI Toolkit, NGUI, custom)
- ✅ **Modular** - Every component can be extended or replaced
- ✅ **Event-Driven** - Clean separation between logic and presentation
- ✅ **Data-Driven** - ScriptableObject-based item definitions
- ✅ **Genre-Neutral** - Perfect for RPG, survival, shooter, mobile, VR, etc.
- ✅ **Production-Ready** - Clean code, full documentation, tested

**The core handles logic. You handle presentation.**

---

## 🚀 Quick Start

### Installation

1. Import the framework into your Unity project
2. The framework is located in `Assets/InventoryFramework/`

### Create Your First Inventory (3 Steps)

**Step 1: Create an Inventory**
```
1. Create empty GameObject in scene
2. Add "Inventory" component
3. Set capacity (e.g., 20 slots)
```

**Step 2: Create an Item**
```
1. Right-click in Project → Create → Inventory Framework → Item Definition
2. Configure: name, icon, max stack size, category, tags
```

**Step 3: Connect to Your Code**
```csharp
using InventoryFramework.Core;

public class MyGame : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemDefinition woodItem;

    void Start()
    {
        // Subscribe to events
        inventory.EventBus.OnItemAdded += (item, amt, slot) => 
        {
            Debug.Log($"Added {amt}x {item.ItemName}");
            // Update your UI here
        };

        // Add an item
        inventory.AddItem(woodItem, 5);
    }
}
```

**That's it!** No UI setup required - connect it to your own UI whenever you're ready.

---

## 📦 What's Included (Milestone 1 - Core Engine)

### Core Systems

| Component | Description |
|-----------|-------------|
| **ItemDefinition** | ScriptableObject for item data (name, icon, stack size, metadata) |
| **InventorySlot** | Individual slot with stacking, metadata, add/remove logic |
| **Inventory** | Main container with capacity, queries, add/remove, swap |
| **InventoryEventBus** | Event system for UI/gameplay integration |
| **InventorySerializer** | JSON persistence (PlayerPrefs, files) with versioning |

### Documentation

- ✅ API Reference (complete)
- ✅ Sample Scripts
- ✅ Integration Examples
- ✅ Architecture Documentation

### Sample Assets

- Example scripts demonstrating usage
- Integration patterns for common scenarios
- Best practices guide

---

## 💡 Key Features

### 1. True UI-Agnostic Design

The framework **never** touches your UI. Instead, it provides events:

```csharp
inventory.EventBus.OnSlotChanged += (slotIndex, slot) => 
{
    // Update YOUR UI element
    myUISlots[slotIndex].Refresh(slot);
};
```

Works with:
- Unity UI (uGUI)
- UI Toolkit
- TextMeshPro
- Custom UI systems
- VR interfaces
- Mobile touch UI

### 2. Powerful Event System

```csharp
OnItemAdded      // When items are added
OnItemRemoved    // When items are removed
OnSlotChanged    // When a slot changes
OnStackChanged   // When stack size changes
OnInventoryChanged // General inventory updates
OnInventoryFull  // When inventory is full
```

### 3. Flexible Metadata System

Add custom data to any item at runtime:

```csharp
slot.SetMetadata("durability", "85");
slot.SetMetadata("enchantment", "fire");
slot.SetMetadata("level", "5");

string durability = slot.GetMetadata("durability");
```

Perfect for:
- Weapon durability
- Ammo counts
- Item quality/rarity
- Temperature (survival games)
- Custom stats

### 4. Complete Serialization

```csharp
// Save to PlayerPrefs
InventorySerializer.SaveToPlayerPrefs(inventory);

// Load from PlayerPrefs
InventorySerializer.LoadFromPlayerPrefs(inventory);

// Or use files
InventorySerializer.SaveToFile(inventory, path);
```

- Automatic metadata serialization
- Version control for updates
- JSON format (human-readable)

### 5. Rich Query API

```csharp
// Check inventory
bool hasWood = inventory.HasItem(woodItem, 10);
int totalWood = inventory.GetItemCount(woodItem);

// Find items
int slotIndex = inventory.FindSlot(woodItem);
var weapons = inventory.GetItemsByCategory("Weapon");
var consumables = inventory.GetItemsByTag("Consumable");

// Manage slots
inventory.SwapSlots(0, 5); // Drag & drop support
inventory.SetCapacity(40); // Dynamic resizing
```

---

## 📁 Project Structure

```
Assets/
├── InventoryFramework/          (main package)
│   ├── Core/                    ✅ MILESTONE 1 - COMPLETE
│   │   ├── ItemDefinition.cs
│   │   ├── InventorySlot.cs
│   │   ├── Inventory.cs
│   │   ├── InventoryEventBus.cs
│   │   └── InventorySerializer.cs
│   ├── Extensions/              📋 Milestone 2 - Coming Soon
│   ├── Editor/                  📋 Milestone 3 - Coming Soon
│   ├── Samples/                 
│   │   ├── Scripts/
│   │   │   └── InventoryExample.cs
│   │   └── README.md
│   └── Documentation/           
│       ├── README.md            (this file)
│       └── API_Reference.md
└── _DevTest/                    (development scratch area)
```

---

## 🎓 Example Usage

### Basic Integration

```csharp
using UnityEngine;
using InventoryFramework.Core;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private Inventory inventory;

    void Start()
    {
        // Connect to your UI
        inventory.EventBus.OnSlotChanged += UpdateSlotUI;
        inventory.EventBus.OnInventoryFull += ShowFullMessage;
    }

    public void PickupItem(ItemDefinition item, int amount)
    {
        int overflow = inventory.AddItem(item, amount);
        if (overflow == 0)
        {
            Debug.Log("All items added!");
        }
    }

    void UpdateSlotUI(int slotIndex, InventorySlot slot)
    {
        // Update your UI here
        if (slot.IsEmpty)
            ClearUISlot(slotIndex);
        else
            SetUISlot(slotIndex, slot.Item.Icon, slot.Quantity);
    }

    void ShowFullMessage(ItemDefinition item, int overflow)
    {
        ShowNotification($"Inventory full! {overflow} {item.ItemName} dropped.");
    }
}
```

### Crafting System Integration

```csharp
public bool TryCraft(CraftingRecipe recipe)
{
    // Check materials
    foreach (var material in recipe.materials)
    {
        if (!inventory.HasItem(material.item, material.amount))
            return false;
    }

    // Consume materials
    foreach (var material in recipe.materials)
    {
        inventory.RemoveItem(material.item, material.amount);
    }

    // Add result
    inventory.AddItem(recipe.result, recipe.resultAmount);
    return true;
}
```

---

## 🛣️ Development Roadmap

### ✅ Milestone 1 - Core Engine (COMPLETE)
- ItemDefinition ScriptableObject
- InventorySlot with stacking & metadata
- Inventory container with full API
- Event-driven system
- JSON serialization

### 📋 Milestone 2 - Extension System (Planned)
- Rule Blocks (modular logic)
- Metadata Channels (typed metadata)
- Slot Types (equipment, hotbar, restricted)
- Weight system enhancements

### 📋 Milestone 3 - UI Binding Wizard (Planned)
- Inspector tool for UI binding
- Auto-detection of UI elements
- Code generation for boilerplate
- Pre-configured profiles (RPG, Survival, Shooter)

---

## 📚 Documentation

- **[API Reference](API_Reference.md)** - Complete API documentation
- **[Samples README](../Samples/README.md)** - Usage examples and integration guide
- **Architecture PDF** - Full framework architecture (in project root)

---

## 🎮 Design Philosophy

1. **UI-Agnostic** - Never force a specific UI approach
2. **Modular** - Every part is independent and replaceable
3. **Predictable** - Clean, consistent behavior
4. **Data-Driven** - ScriptableObjects for easy iteration
5. **Event-Driven** - Clean separation of concerns
6. **Genre-Neutral** - Works for any game type

**The framework provides logic. You provide the presentation.**

---

## 🔧 Requirements

- Unity 2020.3 or newer
- No external dependencies
- Works with any Unity render pipeline

---

## 📝 License

MIT License - See LICENSE file for details

---

## 🤝 Support

- Check the [API Reference](API_Reference.md) for detailed documentation
- See [Samples](../Samples/README.md) for integration examples
- Review the architecture PDF for system design

---

## ✨ Why Choose This Framework?

✅ **Works with YOUR game** - Not the other way around  
✅ **No UI lock-in** - Use any UI system you want  
✅ **Production-ready** - Clean, tested, documented  
✅ **Extensible** - Build on top, don't work around  
✅ **Event-driven** - Clean architecture, easy testing  
✅ **Future-proof** - Designed for long-term projects  

**Start building your inventory system in minutes, not days.**

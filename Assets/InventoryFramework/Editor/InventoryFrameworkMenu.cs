using UnityEngine;
using UnityEditor;
using InventoryFramework.Core;

namespace InventoryFramework.Editor
{
    /// <summary>
    /// Utility menu items and shortcuts for the Inventory Framework
    /// Provides quick access to common operations
    /// </summary>
    public static class InventoryFrameworkMenu
    {
        // Quick GameObject creation
        [MenuItem("GameObject/Inventory Framework/Create Inventory", false, 10)]
        private static void CreateInventory(MenuCommand menuCommand)
        {
            // Create empty GameObject with Inventory component
            GameObject go = new GameObject("Inventory");
            Inventory inventory = go.AddComponent<Inventory>();
            
            // Set default capacity
            SerializedObject so = new SerializedObject(inventory);
            so.FindProperty("capacity").intValue = 20;
            so.ApplyModifiedProperties();
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(go, "Create Inventory");
            
            // Select the new object
            Selection.activeGameObject = go;
            
            Debug.Log("Inventory created! Configure capacity and settings in the Inspector.");
        }

        // Quick GameObject creation for player inventory
        [MenuItem("GameObject/Inventory Framework/Create Player Inventory", false, 11)]
        private static void CreatePlayerInventory(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("PlayerInventory");
            Inventory inventory = go.AddComponent<Inventory>();
            
            // Set typical player inventory capacity
            SerializedObject so = new SerializedObject(inventory);
            so.FindProperty("capacity").intValue = 30;
            so.ApplyModifiedProperties();
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(go, "Create Player Inventory");
            
            // Select
            Selection.activeGameObject = go;
            
            Debug.Log("Player Inventory created with 30 slots. Add the InventoryExample script to test it!");
        }

        // Tools menu
        [MenuItem("Tools/Inventory Framework/Clear All PlayerPrefs Saves")]
        private static void ClearAllSaves()
        {
            if (EditorUtility.DisplayDialog("Clear Inventory Saves", 
                "This will delete all inventory saves from PlayerPrefs. This cannot be undone.\n\nAre you sure?", 
                "Yes, Delete", "Cancel"))
            {
                PlayerPrefs.DeleteKey("InventoryData");
                PlayerPrefs.Save();
                Debug.Log("Inventory saves cleared from PlayerPrefs");
            }
        }

        [MenuItem("Tools/Inventory Framework/Documentation/Open API Reference")]
        private static void OpenAPIReference()
        {
            string path = "Assets/InventoryFramework/Documentation/API_Reference.md";
            if (System.IO.File.Exists(path))
            {
                Application.OpenURL("file:///" + System.IO.Path.GetFullPath(path));
            }
            else
            {
                EditorUtility.DisplayDialog("File Not Found", 
                    "API Reference not found at:\n" + path, "OK");
            }
        }

        [MenuItem("Tools/Inventory Framework/Documentation/Open README")]
        private static void OpenReadme()
        {
            string path = "Assets/InventoryFramework/Documentation/README.md";
            if (System.IO.File.Exists(path))
            {
                Application.OpenURL("file:///" + System.IO.Path.GetFullPath(path));
            }
            else
            {
                EditorUtility.DisplayDialog("File Not Found", 
                    "README not found at:\n" + path, "OK");
            }
        }

        [MenuItem("Tools/Inventory Framework/Documentation/Open Samples Guide")]
        private static void OpenSamplesGuide()
        {
            string path = "Assets/InventoryFramework/Samples/README.md";
            if (System.IO.File.Exists(path))
            {
                Application.OpenURL("file:///" + System.IO.Path.GetFullPath(path));
            }
            else
            {
                EditorUtility.DisplayDialog("File Not Found", 
                    "Samples README not found at:\n" + path, "OK");
            }
        }

        // Validation helpers
        [MenuItem("Tools/Inventory Framework/Validate/Find All Items")]
        private static void FindAllItems()
        {
            string[] guids = AssetDatabase.FindAssets("t:ItemDefinition");
            
            Debug.Log($"=== Found {guids.Length} ItemDefinition(s) ===");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemDefinition item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
                Debug.Log($"- {item.ItemName} ({path})");
            }
            
            if (guids.Length == 0)
            {
                Debug.Log("No items found. Create one with: Right-click > Create > Inventory Framework > Item Definition");
            }
        }

        [MenuItem("Tools/Inventory Framework/Validate/Find All Inventories in Scene")]
        private static void FindAllInventories()
        {
            Inventory[] inventories = Object.FindObjectsByType<Inventory>(FindObjectsInactive.Exclude);
            
            Debug.Log($"=== Found {inventories.Length} Inventory component(s) in scene ===");
            
            foreach (Inventory inv in inventories)
            {
                Debug.Log($"- {inv.gameObject.name} (Capacity: {inv.Capacity}, Used: {inv.UsedSlots})", inv.gameObject);
            }
            
            if (inventories.Length == 0)
            {
                Debug.Log("No inventories found in scene. Create one with: GameObject > Inventory Framework > Create Inventory");
            }
        }

        // Quick setup helper
        [MenuItem("Tools/Inventory Framework/Quick Setup/Create Demo Setup")]
        private static void CreateDemoSetup()
        {
            // Create inventory
            GameObject invGo = new GameObject("DemoInventory");
            Inventory inventory = invGo.AddComponent<Inventory>();
            
            SerializedObject so = new SerializedObject(inventory);
            so.FindProperty("capacity").intValue = 20;
            so.ApplyModifiedProperties();
            
            // Add example script (using Type to avoid assembly reference issues)
            var exampleType = System.Type.GetType("InventoryFramework.Samples.InventoryExample, Assembly-CSharp");
            if (exampleType != null)
            {
                invGo.AddComponent(exampleType);
            }
            
            Undo.RegisterCreatedObjectUndo(invGo, "Create Demo Setup");
            Selection.activeGameObject = invGo;
            
            EditorUtility.DisplayDialog("Demo Setup Created", 
                "Demo inventory created!\n\n" +
                "Next steps:\n" +
                "1. Create some items: Right-click > Create > Inventory Framework > Item Definition\n" +
                "2. Assign items to the InventoryExample component\n" +
                "3. Press Play and use keyboard shortcuts (see Console)\n\n" +
                "Keyboard shortcuts:\n" +
                "1 - Add item 1\n" +
                "2 - Add item 2 (x5)\n" +
                "R - Remove item\n" +
                "C - Clear inventory\n" +
                "S - Save\n" +
                "L - Load\n" +
                "I - Print contents", 
                "OK");
        }

        // About
        [MenuItem("Tools/Inventory Framework/About")]
        private static void ShowAbout()
        {
            EditorUtility.DisplayDialog("Inventory Framework", 
                "Unity Inventory Framework\n" +
                "Version: 1.0.0 (Milestone 1)\n\n" +
                "A complete, modular, UI-agnostic inventory system\n\n" +
                "Features:\n" +
                "✓ UI-Agnostic design\n" +
                "✓ Event-driven architecture\n" +
                "✓ Full persistence system\n" +
                "✓ Extensible metadata\n" +
                "✓ Runtime debugging tools\n\n" +
                "Documentation: Tools > Inventory Framework > Documentation\n" +
                "GitHub: Dfritz22/InventoryFramework", 
                "OK");
        }
    }

    /// <summary>
    /// Asset creation menu for ItemDefinition
    /// This is what shows up in the Create menu
    /// </summary>
    public static class ItemDefinitionAssetCreation
    {
        [MenuItem("Assets/Create/Inventory Framework/Item Definition", false, 80)]
        private static void CreateItemDefinition()
        {
            // Get selected folder path
            string path = "Assets";
            
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (System.IO.File.Exists(path))
                {
                    path = System.IO.Path.GetDirectoryName(path);
                }
                break;
            }

            // Create asset
            ItemDefinition asset = ScriptableObject.CreateInstance<ItemDefinition>();
            
            // Set default values via SerializedObject
            SerializedObject so = new SerializedObject(asset);
            so.FindProperty("itemName").stringValue = "New Item";
            so.FindProperty("maxStackSize").intValue = 1;
            so.ApplyModifiedProperties();

            // Generate unique path
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/NewItem.asset");
            
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select and focus
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        // Quick create common item types
        [MenuItem("Assets/Create/Inventory Framework/Quick Create/Consumable Item", false, 81)]
        private static void CreateConsumableItem()
        {
            CreateQuickItem("NewConsumable", "Consumable", new[] { "Consumable", "Useable" }, 99);
        }

        [MenuItem("Assets/Create/Inventory Framework/Quick Create/Weapon", false, 82)]
        private static void CreateWeaponItem()
        {
            CreateQuickItem("NewWeapon", "Weapon", new[] { "Equipment", "Weapon" }, 1);
        }

        [MenuItem("Assets/Create/Inventory Framework/Quick Create/Material", false, 83)]
        private static void CreateMaterialItem()
        {
            CreateQuickItem("NewMaterial", "Material", new[] { "Crafting", "Material" }, 99);
        }

        private static void CreateQuickItem(string name, string category, string[] tags, int stackSize)
        {
            string path = "Assets";
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (System.IO.File.Exists(path))
                    path = System.IO.Path.GetDirectoryName(path);
                break;
            }

            ItemDefinition asset = ScriptableObject.CreateInstance<ItemDefinition>();
            SerializedObject so = new SerializedObject(asset);
            
            so.FindProperty("itemName").stringValue = name;
            so.FindProperty("category").stringValue = category;
            so.FindProperty("maxStackSize").intValue = stackSize;
            
            var tagsProp = so.FindProperty("tags");
            tagsProp.ClearArray();
            for (int i = 0; i < tags.Length; i++)
            {
                tagsProp.InsertArrayElementAtIndex(i);
                tagsProp.GetArrayElementAtIndex(i).stringValue = tags[i];
            }
            
            so.ApplyModifiedProperties();

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{name}.asset");
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}

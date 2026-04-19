using UnityEngine;
using UnityEditor;
using InventoryFramework.Core;

namespace InventoryFramework.Editor
{
    /// <summary>
    /// Custom editor for Inventory component with adaptive UI based on user settings
    /// Shows simple interface by default, expands to show advanced features when enabled
    /// </summary>
    [CustomEditor(typeof(Inventory))]
    public class InventoryEditor : UnityEditor.Editor
    {
        private SerializedProperty capacity;
        private SerializedProperty useWeightLimit;
        private SerializedProperty maxWeight;
        private SerializedProperty slots;

        private bool showRuntimeInfo = true;
        private bool showSlotDetails = false;
        private bool showAdvancedFeatures = false;

        private Inventory inventory;

        private void OnEnable()
        {
            inventory = (Inventory)target;
            
            // Cache serialized properties
            capacity = serializedObject.FindProperty("capacity");
            useWeightLimit = serializedObject.FindProperty("useWeightLimit");
            maxWeight = serializedObject.FindProperty("maxWeight");
            slots = serializedObject.FindProperty("slots");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header
            DrawCustomHeader();

            EditorGUILayout.Space(10);

            // Basic Configuration
            DrawBasicConfiguration();

            EditorGUILayout.Space(10);

            // Advanced Features (Adaptive - only show if enabled)
            DrawAdvancedFeatures();

            EditorGUILayout.Space(10);

            // Runtime Information (only in play mode)
            if (Application.isPlaying)
            {
                DrawRuntimeInformation();
            }

            // Utility Buttons
            DrawUtilityButtons();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCustomHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Inventory System", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("UI-Agnostic Inventory Container", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawBasicConfiguration()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Basic Configuration", EditorStyles.boldLabel);
            
            // Capacity with helpful range
            EditorGUILayout.PropertyField(capacity, new GUIContent("Capacity", "Total number of inventory slots"));
            
            if (capacity.intValue < 1)
            {
                EditorGUILayout.HelpBox("Capacity must be at least 1", MessageType.Warning);
            }
            else if (capacity.intValue > 100)
            {
                EditorGUILayout.HelpBox("Very large inventories may impact performance. Consider pagination for UI.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedFeatures()
        {
            // Adaptive UI - only show foldout if any advanced features are enabled OR if user wants to see them
            bool hasAdvancedFeatures = useWeightLimit.boolValue;
            
            if (hasAdvancedFeatures || showAdvancedFeatures)
            {
                showAdvancedFeatures = EditorGUILayout.BeginFoldoutHeaderGroup(showAdvancedFeatures, "Advanced Features");
                
                if (showAdvancedFeatures)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    // Weight System Toggle
                    EditorGUILayout.PropertyField(useWeightLimit, new GUIContent("Use Weight Limit", "Enable weight-based inventory restrictions"));
                    
                    // Only show weight max if weight system is enabled (ADAPTIVE!)
                    if (useWeightLimit.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(maxWeight, new GUIContent("Max Weight", "Maximum total weight the inventory can hold"));
                        
                        if (maxWeight.floatValue <= 0)
                        {
                            EditorGUILayout.HelpBox("Max weight should be greater than 0", MessageType.Warning);
                        }
                        
                        // Show current weight in play mode
                        if (Application.isPlaying)
                        {
                            EditorGUILayout.LabelField("Current Weight", inventory.CurrentWeight.ToString("F2"));
                            
                            if (inventory.IsOverweight)
                            {
                                EditorGUILayout.HelpBox("Inventory is currently overweight!", MessageType.Warning);
                            }
                        }
                        
                        EditorGUI.indentLevel--;
                    }
                    
                    EditorGUILayout.Space(5);
                    
                    // Future features placeholder
                    EditorGUILayout.LabelField("Additional Features (Coming in Milestone 2):", EditorStyles.miniLabel);
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Toggle("Rule Blocks", false);
                    EditorGUILayout.Toggle("Slot Types", false);
                    EditorGUILayout.Toggle("Metadata Channels", false);
                    EditorGUI.EndDisabledGroup();
                    
                    EditorGUILayout.EndVertical();
                }
                
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            else
            {
                // Show a button to enable advanced features
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Advanced Features", EditorStyles.miniLabel);
                if (GUILayout.Button("Show", GUILayout.Width(60)))
                {
                    showAdvancedFeatures = true;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawRuntimeInformation()
        {
            showRuntimeInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showRuntimeInfo, "Runtime Information");
            
            if (showRuntimeInfo)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Summary stats
                EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Used Slots", $"{inventory.UsedSlots} / {inventory.Capacity}");
                EditorGUILayout.LabelField("Free Slots", inventory.FreeSlots.ToString());
                
                // Progress bar for capacity
                Rect rect = EditorGUILayout.GetControlRect(false, 20);
                float fillPercent = inventory.Capacity > 0 ? (float)inventory.UsedSlots / inventory.Capacity : 0;
                EditorGUI.ProgressBar(rect, fillPercent, $"{inventory.UsedSlots}/{inventory.Capacity} slots");
                
                EditorGUILayout.Space(5);
                
                // Weight info if enabled
                if (useWeightLimit.boolValue)
                {
                    EditorGUILayout.LabelField("Weight", $"{inventory.CurrentWeight:F2} / {inventory.MaxWeight:F2}");
                    
                    rect = EditorGUILayout.GetControlRect(false, 20);
                    float weightPercent = inventory.MaxWeight > 0 ? inventory.CurrentWeight / inventory.MaxWeight : 0;
                    EditorGUI.ProgressBar(rect, weightPercent, $"{inventory.CurrentWeight:F2}/{inventory.MaxWeight:F2}");
                }
                
                EditorGUILayout.Space(10);
                
                // Slot contents
                showSlotDetails = EditorGUILayout.Foldout(showSlotDetails, "Slot Contents", true);
                if (showSlotDetails)
                {
                    EditorGUI.indentLevel++;
                    
                    if (inventory.Slots != null && inventory.Slots.Count > 0)
                    {
                        int displayCount = 0;
                        for (int i = 0; i < inventory.Slots.Count && displayCount < 20; i++)
                        {
                            var slot = inventory.Slots[i];
                            if (!slot.IsEmpty)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField($"Slot {i}:", GUILayout.Width(60));
                                EditorGUILayout.LabelField($"{slot.Item.ItemName} x{slot.Quantity}");
                                EditorGUILayout.EndHorizontal();
                                displayCount++;
                            }
                        }
                        
                        if (displayCount == 0)
                        {
                            EditorGUILayout.LabelField("Inventory is empty", EditorStyles.miniLabel);
                        }
                        else if (inventory.UsedSlots > 20)
                        {
                            EditorGUILayout.LabelField($"... and {inventory.UsedSlots - 20} more items", EditorStyles.miniLabel);
                        }
                    }
                    
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawUtilityButtons()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            // Open Inventory Viewer Window
            if (GUILayout.Button("Open Inventory Viewer", GUILayout.Height(25)))
            {
                InventoryViewerWindow.ShowWindow(inventory);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Play mode utilities
            if (Application.isPlaying)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Clear All Slots"))
                {
                    if (EditorUtility.DisplayDialog("Clear Inventory", 
                        "Are you sure you want to clear all slots?", "Yes", "Cancel"))
                    {
                        inventory.ClearAll();
                    }
                }
                
                if (GUILayout.Button("Save to PlayerPrefs"))
                {
                    InventorySerializer.SaveToPlayerPrefs(inventory);
                    Debug.Log("Inventory saved to PlayerPrefs");
                }
                
                if (GUILayout.Button("Load from PlayerPrefs"))
                {
                    if (InventorySerializer.LoadFromPlayerPrefs(inventory))
                        Debug.Log("Inventory loaded from PlayerPrefs");
                    else
                        Debug.Log("No save data found");
                }
                
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Additional utilities available in Play Mode", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }
    }
}

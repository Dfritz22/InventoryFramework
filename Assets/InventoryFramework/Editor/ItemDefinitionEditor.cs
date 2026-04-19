using UnityEngine;
using UnityEditor;
using InventoryFramework.Core;

namespace InventoryFramework.Editor
{
    /// <summary>
    /// Custom editor for ItemDefinition to provide a clean, organized Inspector experience
    /// </summary>
    [CustomEditor(typeof(ItemDefinition))]
    public class ItemDefinitionEditor : UnityEditor.Editor
    {
        private SerializedProperty itemName;
        private SerializedProperty description;
        private SerializedProperty icon;
        private SerializedProperty maxStackSize;
        private SerializedProperty category;
        private SerializedProperty tags;
        private SerializedProperty baseMetadata;

        private bool showAdvancedSettings = false;
        private bool showMetadata = false;

        private void OnEnable()
        {
            // Cache serialized properties
            itemName = serializedObject.FindProperty("itemName");
            description = serializedObject.FindProperty("description");
            icon = serializedObject.FindProperty("icon");
            maxStackSize = serializedObject.FindProperty("maxStackSize");
            category = serializedObject.FindProperty("category");
            tags = serializedObject.FindProperty("tags");
            baseMetadata = serializedObject.FindProperty("baseMetadata");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header with icon preview
            DrawCustomHeader();

            EditorGUILayout.Space(10);

            // Basic Info Section
            DrawBasicInfo();

            EditorGUILayout.Space(10);

            // Stacking Section
            DrawStackingInfo();

            EditorGUILayout.Space(10);

            // Advanced Settings (Foldout)
            DrawAdvancedSettings();

            EditorGUILayout.Space(10);

            // Metadata Section (Foldout)
            DrawMetadataSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCustomHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Item Definition", EditorStyles.boldLabel);
            
            // Preview icon if available
            if (icon.objectReferenceValue != null)
            {
                Sprite iconSprite = icon.objectReferenceValue as Sprite;
                if (iconSprite != null)
                {
                    Rect rect = GUILayoutUtility.GetRect(64, 64, GUILayout.Width(64));
                    GUI.DrawTexture(rect, iconSprite.texture, ScaleMode.ScaleToFit);
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawBasicInfo()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Basic Information", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(itemName, new GUIContent("Item Name", "Display name shown to players"));
            EditorGUILayout.PropertyField(description, new GUIContent("Description", "Item description or tooltip text"));
            EditorGUILayout.PropertyField(icon, new GUIContent("Icon", "Item icon sprite"));
            
            EditorGUILayout.EndVertical();
        }

        private void DrawStackingInfo()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Stacking", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(maxStackSize, new GUIContent("Max Stack Size", "Maximum items per stack (1 = non-stackable)"));
            
            // Helpful hint
            if (maxStackSize.intValue == 1)
            {
                EditorGUILayout.HelpBox("Stack size of 1 means this item cannot stack (unique items, equipment, etc.)", MessageType.Info);
            }
            else if (maxStackSize.intValue > 999)
            {
                EditorGUILayout.HelpBox("Very large stack sizes may cause UI display issues. Consider keeping under 1000.", MessageType.Warning);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedSettings()
        {
            showAdvancedSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showAdvancedSettings, "Classification & Tags");
            
            if (showAdvancedSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.PropertyField(category, new GUIContent("Category", "Item category (e.g., 'Weapon', 'Consumable', 'Material')"));
                
                EditorGUILayout.Space(5);
                
                // Tags with helpful description
                EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Tags allow flexible querying (e.g., 'Stackable', 'Tradeable', 'QuestItem')", MessageType.Info);
                EditorGUILayout.PropertyField(tags, new GUIContent(""), true);
                
                // Quick tag buttons
                if (tags.isExpanded)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("+ Consumable", GUILayout.Width(100)))
                        AddTag("Consumable");
                    if (GUILayout.Button("+ Tradeable", GUILayout.Width(100)))
                        AddTag("Tradeable");
                    if (GUILayout.Button("+ Rare", GUILayout.Width(100)))
                        AddTag("Rare");
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawMetadataSection()
        {
            showMetadata = EditorGUILayout.BeginFoldoutHeaderGroup(showMetadata, "Base Metadata (Optional)");
            
            if (showMetadata)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.HelpBox("Base metadata provides default values for all instances of this item. Runtime metadata can override these.", MessageType.Info);
                
                EditorGUILayout.PropertyField(baseMetadata, new GUIContent("Metadata Entries"), true);
                
                // Quick metadata buttons
                if (baseMetadata.isExpanded)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Quick Add:", EditorStyles.miniLabel);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("+ Durability", GUILayout.Width(100)))
                        AddMetadata("durability", "100");
                    if (GUILayout.Button("+ Weight", GUILayout.Width(100)))
                        AddMetadata("weight", "1.0");
                    if (GUILayout.Button("+ Value", GUILayout.Width(100)))
                        AddMetadata("value", "10");
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void AddTag(string tag)
        {
            int size = tags.arraySize;
            
            // Check if tag already exists
            for (int i = 0; i < size; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                    return; // Already exists
            }
            
            tags.InsertArrayElementAtIndex(size);
            tags.GetArrayElementAtIndex(size).stringValue = tag;
            serializedObject.ApplyModifiedProperties();
        }

        private void AddMetadata(string key, string value)
        {
            int size = baseMetadata.arraySize;
            
            // Check if key already exists
            for (int i = 0; i < size; i++)
            {
                var entry = baseMetadata.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("key").stringValue == key)
                    return; // Already exists
            }
            
            baseMetadata.InsertArrayElementAtIndex(size);
            var newEntry = baseMetadata.GetArrayElementAtIndex(size);
            newEntry.FindPropertyRelative("key").stringValue = key;
            newEntry.FindPropertyRelative("value").stringValue = value;
            serializedObject.ApplyModifiedProperties();
        }
    }
}

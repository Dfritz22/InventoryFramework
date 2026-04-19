using UnityEngine;
using UnityEditor;
using InventoryFramework.Core;

namespace InventoryFramework.Editor
{
    /// <summary>
    /// Editor window for viewing and debugging inventory contents at runtime
    /// Provides a visual grid view of all slots with their contents
    /// </summary>
    public class InventoryViewerWindow : EditorWindow
    {
        private Inventory targetInventory;
        private Vector2 scrollPosition;
        private const float SLOT_SIZE = 60f;
        private const float SLOT_SPACING = 5f;

        [MenuItem("Window/Inventory Framework/Inventory Viewer")]
        public static void ShowWindow()
        {
            var window = GetWindow<InventoryViewerWindow>("Inventory Viewer");
            window.minSize = new Vector2(400, 300);
        }

        public static void ShowWindow(Inventory inventory)
        {
            var window = GetWindow<InventoryViewerWindow>("Inventory Viewer");
            window.targetInventory = inventory;
            window.minSize = new Vector2(400, 300);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            // Header
            DrawHeader();

            EditorGUILayout.Space(10);

            // Inventory selector
            DrawInventorySelector();

            EditorGUILayout.Space(10);

            // Main content
            if (targetInventory != null && Application.isPlaying)
            {
                DrawInventoryGrid();
            }
            else if (targetInventory != null && !Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to view inventory contents", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Select an Inventory component to view its contents", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Inventory Viewer", EditorStyles.boldLabel);
            
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    Repaint();
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawInventorySelector()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            Inventory newTarget = EditorGUILayout.ObjectField("Target Inventory", targetInventory, typeof(Inventory), true) as Inventory;
            
            if (newTarget != targetInventory)
            {
                targetInventory = newTarget;
                Repaint();
            }

            // Quick find button
            if (targetInventory == null)
            {
                if (GUILayout.Button("Find in Scene"))
                {
                    targetInventory = Object.FindFirstObjectByType<Inventory>();
                    if (targetInventory != null)
                    {
                        Debug.Log($"Found inventory: {targetInventory.gameObject.name}");
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawInventoryGrid()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Stats header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Capacity: {targetInventory.Capacity}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Used: {targetInventory.UsedSlots}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Free: {targetInventory.FreeSlots}", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Scrollable grid
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            int columns = Mathf.Max(1, Mathf.FloorToInt((position.width - 40) / (SLOT_SIZE + SLOT_SPACING)));
            int rows = Mathf.CeilToInt((float)targetInventory.Capacity / columns);

            for (int row = 0; row < rows; row++)
            {
                EditorGUILayout.BeginHorizontal();

                for (int col = 0; col < columns; col++)
                {
                    int index = row * columns + col;
                    if (index >= targetInventory.Capacity)
                        break;

                    DrawSlot(index, targetInventory.Slots[index]);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(SLOT_SPACING);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawSlot(int index, InventorySlot slot)
        {
            Rect rect = GUILayoutUtility.GetRect(SLOT_SIZE, SLOT_SIZE);
            
            // Background
            Color bgColor = slot.IsEmpty ? new Color(0.2f, 0.2f, 0.2f, 0.5f) : new Color(0.3f, 0.4f, 0.5f, 0.8f);
            EditorGUI.DrawRect(rect, bgColor);

            // Border
            Handles.DrawSolidRectangleWithOutline(rect, Color.clear, new Color(0.5f, 0.5f, 0.5f, 1f));

            if (!slot.IsEmpty)
            {
                // Icon
                if (slot.Item.Icon != null)
                {
                    Rect iconRect = new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 20);
                    GUI.DrawTexture(iconRect, slot.Item.Icon.texture, ScaleMode.ScaleToFit);
                }

                // Quantity
                GUIStyle quantityStyle = new GUIStyle(EditorStyles.boldLabel);
                quantityStyle.alignment = TextAnchor.LowerRight;
                quantityStyle.normal.textColor = Color.white;
                quantityStyle.fontSize = 10;

                Rect quantityRect = new Rect(rect.x, rect.y + rect.height - 18, rect.width - 3, 15);
                GUI.Label(quantityRect, $"x{slot.Quantity}", quantityStyle);

                // Tooltip on hover
                if (rect.Contains(Event.current.mousePosition))
                {
                    DrawSlotTooltip(slot);
                }
            }
            else
            {
                // Slot index for empty slots
                GUIStyle indexStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                indexStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                GUI.Label(rect, index.ToString(), indexStyle);
            }
        }

        private void DrawSlotTooltip(InventorySlot slot)
        {
            // Show tooltip with item details
            string tooltip = $"{slot.Item.ItemName}\n";
            tooltip += $"Quantity: {slot.Quantity}\n";
            
            if (!string.IsNullOrEmpty(slot.Item.Category))
                tooltip += $"Category: {slot.Item.Category}\n";

            if (slot.Item.Tags.Count > 0)
                tooltip += $"Tags: {string.Join(", ", slot.Item.Tags)}\n";

            if (slot.RuntimeMetadata != null && slot.RuntimeMetadata.Count > 0)
            {
                tooltip += "\nMetadata:\n";
                foreach (var kvp in slot.RuntimeMetadata)
                {
                    tooltip += $"  {kvp.Key}: {kvp.Value}\n";
                }
            }

            // Draw tooltip
            Vector2 mousePos = Event.current.mousePosition;
            GUIStyle tooltipStyle = new GUIStyle(EditorStyles.helpBox);
            tooltipStyle.richText = true;
            
            Vector2 size = tooltipStyle.CalcSize(new GUIContent(tooltip));
            Rect tooltipRect = new Rect(mousePos.x + 15, mousePos.y, size.x + 10, size.y + 10);
            
            GUI.Box(tooltipRect, tooltip, tooltipStyle);
        }

        private void OnInspectorUpdate()
        {
            // Auto-refresh during play mode
            if (Application.isPlaying && targetInventory != null)
            {
                Repaint();
            }
        }
    }
}

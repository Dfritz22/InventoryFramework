using UnityEngine;
using UnityEditor;

namespace InventoryFramework.Editor
{
    /// <summary>
    /// Welcome screen that appears when the framework is first used
    /// Provides quick start guide and helpful links
    /// </summary>
    public class WelcomeWindow : EditorWindow
    {
        private const string SHOWN_PREF_KEY = "InventoryFramework_WelcomeShown";
        private Vector2 scrollPosition;

        [InitializeOnLoadMethod]
        private static void OnProjectLoadedInEditor()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool(SHOWN_PREF_KEY, false))
                {
                    ShowWindow();
                    SessionState.SetBool(SHOWN_PREF_KEY, true);
                }
            };
        }

        [MenuItem("Window/Inventory Framework/Welcome Screen")]
        public static void ShowWindow()
        {
            WelcomeWindow window = GetWindow<WelcomeWindow>("Welcome");
            window.minSize = new Vector2(500, 600);
            window.maxSize = new Vector2(500, 600);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Header
            EditorGUILayout.Space(10);
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 20;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("Unity Inventory Framework", titleStyle);
            
            GUIStyle subtitleStyle = new GUIStyle(EditorStyles.label);
            subtitleStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("A Complete, UI-Agnostic Inventory System", subtitleStyle);

            EditorGUILayout.Space(20);

            // Version info
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Version: 1.0.0 (Milestone 1 - Core Engine)", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Quick Start
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Quick Start Guide", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("1. Create an Inventory", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("   GameObject > Inventory Framework > Create Inventory");
            EditorGUILayout.Space(3);

            EditorGUILayout.LabelField("2. Create Items", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("   Right-click in Project > Create > Inventory Framework > Item Definition");
            EditorGUILayout.Space(3);

            EditorGUILayout.LabelField("3. Connect to Your Code", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("   Subscribe to inventory.EventBus events for UI integration");
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Create Demo Setup Now", GUILayout.Height(30)))
            {
                CreateDemoSetup();
            }
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Key Features
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Key Features", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            DrawFeature("✓", "UI-Agnostic - Works with any UI system");
            DrawFeature("✓", "Event-Driven - Clean separation of logic and presentation");
            DrawFeature("✓", "Modular - Every component is extensible");
            DrawFeature("✓", "Data-Driven - ScriptableObject-based items");
            DrawFeature("✓", "Persistent - Built-in save/load system");
            DrawFeature("✓", "Genre-Neutral - Perfect for any game type");
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Tools & Resources
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Tools & Resources", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (GUILayout.Button("Open Inventory Viewer"))
            {
                InventoryViewerWindow.ShowWindow();
            }

            if (GUILayout.Button("Open API Reference"))
            {
                OpenAPIReference();
            }

            if (GUILayout.Button("Open Documentation"))
            {
                OpenDocumentation();
            }

            if (GUILayout.Button("Open Samples Guide"))
            {
                OpenSamplesGuide();
            }
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Integration Example
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Integration Example", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            GUIStyle codeStyle = new GUIStyle(EditorStyles.textArea);
            codeStyle.wordWrap = true;
            
            string code = @"// Connect to your UI
inventory.EventBus.OnSlotChanged += (slot, index) => {
    myUI.UpdateSlot(slot, index);
};

// Add items
inventory.AddItem(woodItem, 10);

// Check inventory
if (inventory.HasItem(keyItem)) {
    OpenDoor();
}";

            EditorGUILayout.TextArea(code, codeStyle);
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Next Steps
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Coming Soon (Milestone 2)", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField("• Rule Blocks - Modular behavior system");
            EditorGUILayout.LabelField("• Metadata Channels - Typed metadata");
            EditorGUILayout.LabelField("• Slot Types - Equipment, hotbar, restrictions");
            EditorGUILayout.LabelField("• Smart Stacking - Advanced stacking rules");
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Footer
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Don't Show Again"))
            {
                EditorPrefs.SetBool(SHOWN_PREF_KEY, true);
                Close();
            }
            if (GUILayout.Button("Close"))
            {
                Close();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUILayout.EndScrollView();
        }

        private void DrawFeature(string icon, string text)
        {
            EditorGUILayout.BeginHorizontal();
            GUIStyle iconStyle = new GUIStyle(EditorStyles.label);
            iconStyle.normal.textColor = Color.green;
            EditorGUILayout.LabelField(icon, iconStyle, GUILayout.Width(20));
            EditorGUILayout.LabelField(text);
            EditorGUILayout.EndHorizontal();
        }

        private void CreateDemoSetup()
        {
            Close();
            EditorApplication.delayCall += () =>
            {
                // Use the menu command
                EditorApplication.ExecuteMenuItem("Tools/Inventory Framework/Quick Setup/Create Demo Setup");
            };
        }

        private void OpenAPIReference()
        {
            string path = "Assets/InventoryFramework/Documentation/API_Reference.md";
            if (System.IO.File.Exists(path))
            {
                Application.OpenURL("file:///" + System.IO.Path.GetFullPath(path));
            }
        }

        private void OpenDocumentation()
        {
            string path = "Assets/InventoryFramework/Documentation/README.md";
            if (System.IO.File.Exists(path))
            {
                Application.OpenURL("file:///" + System.IO.Path.GetFullPath(path));
            }
        }

        private void OpenSamplesGuide()
        {
            string path = "Assets/InventoryFramework/Samples/README.md";
            if (System.IO.File.Exists(path))
            {
                Application.OpenURL("file:///" + System.IO.Path.GetFullPath(path));
            }
        }
    }
}

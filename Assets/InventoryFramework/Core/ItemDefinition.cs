using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryFramework.Core
{
    /// <summary>
    /// ScriptableObject that defines all base data for an item.
    /// This is the foundation of the data-driven approach.
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory Framework/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string itemName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;

        [Header("Stacking")]
        [SerializeField] private int maxStackSize = 1;
        
        [Header("Classification")]
        [SerializeField] private string category;
        [SerializeField] private List<string> tags = new List<string>();

        [Header("Metadata")]
        [SerializeField] private List<MetadataEntry> baseMetadata = new List<MetadataEntry>();

        // Public Properties
        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public int MaxStackSize => maxStackSize;
        public string Category => category;
        public IReadOnlyList<string> Tags => tags;

        /// <summary>
        /// Get base metadata as a dictionary for runtime use
        /// </summary>
        public Dictionary<string, string> GetBaseMetadata()
        {
            var metadata = new Dictionary<string, string>();
            foreach (var entry in baseMetadata)
            {
                if (!string.IsNullOrEmpty(entry.key))
                {
                    metadata[entry.key] = entry.value;
                }
            }
            return metadata;
        }

        /// <summary>
        /// Check if item has a specific tag
        /// </summary>
        public bool HasTag(string tag)
        {
            return tags.Contains(tag);
        }

        /// <summary>
        /// Check if item belongs to a specific category
        /// </summary>
        public bool IsCategory(string cat)
        {
            return string.Equals(category, cat, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Serializable key-value pair for metadata
        /// </summary>
        [Serializable]
        public class MetadataEntry
        {
            public string key;
            public string value;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure max stack size is at least 1
            if (maxStackSize < 1)
            {
                maxStackSize = 1;
            }
        }
#endif
    }
}

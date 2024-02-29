using System.Collections.Generic;
using UnityEngine;

namespace ColorValidation
{
    [System.Serializable]
    public class NameColorMapping
    {
        [SerializeField]
        private string name;
        public string Name => name;

        [SerializeField]
        private Color color = Color.white;
        public Color Color => color; 
    }

    [CreateAssetMenu(menuName = "Color Names")]
    public class ColorNames : ScriptableObject
    {
        [SerializeField]
        private NameColorMapping[] colors;

        private Dictionary<string, Color> mappings;
        public bool TryGetColor(string name, out Color color)
        {
            if (mappings == null)
            {
                mappings = new Dictionary<string, Color>();
                for (int i = 0; i < colors.Length; i++)
                    mappings.Add(colors[i].Name, colors[i].Color);
            }
            return mappings.TryGetValue(name, out color);
        }
    }
}

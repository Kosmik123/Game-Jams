using UnityEditor;
using UnityEngine;

namespace Bipolar.Editor
{
    [CustomPropertyDrawer(typeof(CreateAssetButtonAttribute))]
    public class CreateAssetButtonAttributeDrawer : ButtonAttributePropertyDrawer
    {
        protected override string ButtonText => "New";

        protected override void OnClick()
        {
            Debug.Log("Created");
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace Bipolar.Editor
{
    [CustomPropertyDrawer(typeof(AddComponentButtonAttribute))]
    public class AddComponentButtonAttributeDrawer : ButtonAttributePropertyDrawer
    {
        protected override string ButtonText => "Add";

        protected override void OnClick()
        {
            Debug.Log("Added");
        }
    }
}

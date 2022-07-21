using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MySOContainer : ScriptableObject
{
    public string Termometer = "hot stuff"; // example normal data serialization along side 

    public MySOItem Preview; // Try this field , drag and drop the items here, or use the pop up list browser window 

    [SerializeField]  // avoid extra difficulty by keeping a serialized reference list for the items 
    [HideInInspector] // better keep this hiddne to avoid user modification , can be commented for debugging 
    List<MySOItem> nested = new List<MySOItem>();

    #if UNITY_EDITOR

    // Note the function names has the "editor" keyword - this should give you a hint not to use it outside of editor scripts 

    public void EditorCreate( string itemName )
    {
        var item = CreateInstance<MySOItem>();

        item.name = itemName; // asset name on disc 

        // optional 
        {
            // lets give it some random data 
            item.valBool = Random.value > 0.5;
            item.valFloat = Random.value;
            item.valInt = (int) ( ( item.valFloat - .777f ) * 1000 );
        }

        nested.Add( item ); // track reference 

        Preview = item; // optional 

        UnityEditor.AssetDatabase.AddObjectToAsset(item, this);
        UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
    }

    [UnityEditor.CustomEditor(typeof(MySOContainer))]
    class SOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var so = ((MySOContainer)target);

            if ( GUILayout.Button("Add") )
            {
                var new_item_name = Random.value.ToString("N4").Substring(2);

                so.EditorCreate( new_item_name );
            }

            // example : delete first nested SO element ( if such exists ) 

            if (so.nested.Count > 0 && GUILayout.Button("Remove 0"))
            {
                var first = so.nested[0];

                if( so.Preview == first ) so.Preview = null;

                so.nested.Remove(first);

                UnityEditor.AssetDatabase.RemoveObjectFromAsset(first);
                UnityEditor.AssetDatabase.SaveAssets(); // slower method 
            }
        }
    }
    
    #endif
}

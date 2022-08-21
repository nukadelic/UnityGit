using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Overlays;
#endif

[DisallowMultipleComponent]
public class QuickOverlayExample : MonoBehaviour
{

    // normal mono stuff here 

#if UNITY_EDITOR
    [Overlay(typeof(SceneView) , Title )]
    class QuickOverlay : IMGUIOverlay , ITransientOverlay
    {
        QuickOverlayExample target; 

        void BurnToScreen() // some dummy ui 
        {
            if ( GUILayout.Button("Action") ) { }

            GUILayout.Label("YO");
        }

        #region Transient Overlay
        const string Title = "Skinned Mesh Vertex";
        public override void OnGUI() { BurnToScreen(); Update(); } 
        public override void OnCreated() { Selection.selectionChanged += Update; Update(); }
        public override void OnWillBeDestroyed() { Selection.selectionChanged -= Update; Update(); }
        public bool visible => isVisible;
        bool isVisible = false;
        void Update() {
            if( Selection.activeGameObject == null ) isVisible = ( false );
            else if( Selection.activeGameObject == ( target?.gameObject ?? null ) ) isVisible = ( true );
            else if ( ! Selection.activeGameObject.TryGetComponent( out target ) ) isVisible = ( false );
            else isVisible = ( true );
        }
        #endregion
    }
    #endif
}

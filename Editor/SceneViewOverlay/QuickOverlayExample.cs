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

    // editor will crash when seting visability directly in the context-menu inside the
    // scene-view ( press space to show context menu popup ) 
    [Overlay(typeof(SceneView) , YouHaveBeenWarned )]
    class QuickOverlay : IMGUIOverlay
    {
        QuickOverlayExample target; 

        void BurnToScreen() // some dummy ui 
        {
            if ( GUILayout.Button("Action") ) { }

            GUILayout.Label("YO");
        }

        #region Fuckery
        public override void OnGUI() { BurnToScreen(); Update(); } 
        public override void OnCreated() { Selection.selectionChanged += Update; Update(); }
        public override void OnWillBeDestroyed() { Selection.selectionChanged -= Update; Update(); }
        const string YouHaveBeenWarned = "Click to crash editor";
        void Update()
        {
            if( Selection.activeGameObject == null ) Show( false );
            else if ( ! Selection.activeGameObject.TryGetComponent( out target ) ) Show( false );
            else Show( true );
        }
        void Show( bool b )
        {
            if( displayed == b ) return;
            displayName = b ? "_" : YouHaveBeenWarned; // empty string will yield errors 
            if ( floatingPosition.magnitude < 50 ) floatingPosition = new Vector2( 100, 100 );
            displayed = b;
        }
        #endregion
    }
    #endif
}

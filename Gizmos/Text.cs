using UnityEngine;

public static partial class Gizmo
{
    #region Text Label


    public static void TextWB( Vector3 p , string s , int lineOffset = 0 ) => Text(p, s, lineOffset, color: Color.white, background: Color.black);
    public static void TextYB( Vector3 p , string s , int lineOffset = 0 ) => Text(p, s, lineOffset, color: Color.yellow, background: Color.black);

    public static void Text( Vector3 position, string text, int lineOffset = 0, Color color = default, Color background = default, bool bold = false , int size = 14 )
    {
        #region code

#if UNITY_EDITOR

        GUIStyle style = new GUIStyle( UnityEditor.EditorStyles.label ); 
        style.normal.textColor = color;
        style.normal.background = Texture2D.blackTexture;
        style.normal.scaledBackgrounds = new Texture2D[] { style.normal.background };
        style.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        style.richText = true;
        style.fontSize = size;
        style.font = GetMonospaceFont();

        var cam = UnityEditor.SceneView.currentDrawingSceneView.camera;

        var up = cam.transform.up;

        var lineHeight = UnityEditor.HandleUtility.GetHandleSize( position ) / 6f * (size / 12f);

        var pos = position + up * lineHeight * lineOffset * -1;

        var content = new GUIContent( text );

        if( background != default )
        {
            var screen_size = style.CalcSize( content );

            var A = cam.WorldToScreenPoint( pos );
            var B = A + new Vector3( screen_size.x, 0 );
            var C = A + new Vector3( screen_size.x, -screen_size.y );
            var D = A + new Vector3( 0, -screen_size.y );

            var shift = lineHeight * 0.2f * up;

            B = cam.ScreenToWorldPoint( B ) + shift;
            C = cam.ScreenToWorldPoint( C ) + shift;
            D = cam.ScreenToWorldPoint( D ) + shift;

            var verts = new Vector3[] { pos + shift, B, C, D };

            UnityEditor.Handles.DrawSolidRectangleWithOutline( verts , background, background );
        }

        UnityEditor.Handles.Label( pos, content , style );

#endif

        #endregion
    }

    #endregion

    #region Font

    static Font monospace;
    static Font GetMonospaceFont()
    {
        if (monospace == null)
            monospace = UnityEditor.EditorGUIUtility.Load("Fonts/RobotoMono/RobotoMono-Regular.ttf") as Font;
        return monospace;
    }

    #endregion

}

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

/**
 * Can't be used if the property is a member of another class 
 * 
 * use left mouse click to drag , scroll wheel to zoom 
 * 
 */

[System.Serializable]
public class InspectorGraph 
{
    public System.Func<float, float> function = null;
    
    public Skin skin;

    [System.Serializable]
    public class Skin
    {
        public Vector2 rangeX = new Vector2(0, 1);
        public Vector2 rangeY = new Vector2(0, 1);
        public float gridSize = 20f;
        public Color gridColor = Color.grey;
        public float drawHeight = 50f;
        public bool fillBackground = true;
        public Color customBackground = default;
        public bool drawAxis = true;
        public Color lineColor = Color.yellow;
        public float lineThickness = 3f;
        public bool mouseDragAndZoom = true;
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof( InspectorGraph ))]
    class PropertyDrawer2 : PropertyDrawer
    {
        public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
        {
            return property.FindPropertyRelative("skin.drawHeight").floatValue;
        }

        readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        Vector2 mouseOffset = Vector2.zero;
        bool mouseDrag = false;
        Vector2 mouseStart = Vector2.zero;
        Rect position;
        float zoom = 1f;

        void RenderLine(float x1, float y1, float x2, float y2, Color c , float t = 1 )
        {
            Handles.color = c;

            Vector2 small = new Vector2(1e-4f, 1e-4f);

            bool outOfBounds1 = ! position.Contains(new Vector2(x1, y1) + small);
            bool outOfBounds2 = ! position.Contains(new Vector2(x2, y2) - small);
            
            if (outOfBounds1 && outOfBounds2 ) return;

            x1 = Mathf.Clamp( x1 , position.xMin, position.xMax );
            x2 = Mathf.Clamp( x2 , position.xMin, position.xMax );
            y1 = Mathf.Clamp( y1 , position.yMin, position.yMax );
            y2 = Mathf.Clamp( y2 , position.yMin, position.yMax );

            if( t != 1 ) Handles.DrawAAPolyLine( t, new Vector3[] { new Vector3( x1, y1, 0 ), new Vector3( x2, y2, 0 ) } );

            else Handles.DrawLine( new Vector3( x1, y1, 0 ), new Vector3( x2, y2, 0 ) );
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if( property == null ) return;

            this.position = position;

            float DPI = EditorGUIUtility.pixelsPerPoint; // normally its = 1 , todo add support for retina display 

            bool isDark = EditorGUIUtility.isProSkin;

            Skin skin = null;

            bool needsRepaint = false;

            System.Func<float, float> func = null;

            if ( property.serializedObject != null && property.serializedObject.targetObject != null )
            {
                object obj = property.serializedObject.targetObject;
                var obj_val = obj.GetType().GetField( property.name, Flags ).GetValue( obj );
                func = (System.Func<float, float>) obj_val.GetType().GetField( "function" , Flags ).GetValue( obj_val );
                skin = ( Skin ) obj_val.GetType().GetField("skin", Flags).GetValue( obj_val );
            }
            else return;

            if ( skin.fillBackground )
            {
                var backgroundColor = isDark ? Color.black : Color.white;
                
                if( skin.customBackground != default ) backgroundColor = skin.customBackground;

                Handles.DrawSolidRectangleWithOutline( position, backgroundColor , backgroundColor );
            }

            

            var currentEvent = Event.current;
            
            var mouse = currentEvent.mousePosition;

            var mouseInBounds = position.Contains( mouse );

            if( currentEvent.type == EventType.MouseDown && currentEvent.button == 1 && mouseInBounds )
            {
                zoom = 1f;

                mouseOffset = Vector2.zero;

                needsRepaint = true;
            }
            if( currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && mouseInBounds )
            {
                mouseDrag = true;

                mouseStart = mouse;
            }
            if( currentEvent.type == EventType.MouseUp || ( currentEvent.isMouse && ! mouseInBounds ) )
            {
                mouseDrag = false;
            }
            if( mouseDrag )
            {
                mouseOffset += ( mouse - mouseStart ) / 1000f * zoom ;

                mouseStart = mouse;

                needsRepaint = true;
            }
            if( currentEvent.type == EventType.ScrollWheel && mouseInBounds )
            {
                zoom += currentEvent.delta.y / 25f;

                currentEvent.Use();

                needsRepaint = true;
            }

            float minX, maxX, minY, maxY;
            Vector2 range;

            if( skin.mouseDragAndZoom )
            {
                skin.rangeX = new Vector2( Mathf.Min( skin.rangeX[0], skin.rangeX[1] ), Mathf.Max( skin.rangeX[0], skin.rangeX[1] ) );
                skin.rangeY = new Vector2( Mathf.Min( skin.rangeY[0], skin.rangeY[1] ), Mathf.Max( skin.rangeY[0], skin.rangeY[1] ) );

                range = new Vector2( skin.rangeX[1] - skin.rangeX[0] , skin.rangeY[1] - skin.rangeY[0] );

                var mouse_offset = new Vector2( mouseOffset.x * range.x , - mouseOffset.y * range.y * 5f );
            
                minX = skin.rangeX[0] - mouse_offset.x; 
                maxX = skin.rangeX[1] - mouse_offset.x;

                minY = skin.rangeY[0] - mouse_offset.y;
                maxY = skin.rangeY[1] - mouse_offset.y;
            
                range = new Vector2( maxX - minX, maxY - minY );

                var mid = new Vector2( minX + range.x / 2f , minY + range.y / 2f );

                minX = mid.x - zoom * range.x / 2f;
                minY = mid.y - zoom * range.y / 2f;
            
                maxX = mid.x + zoom * range.x / 2f;
                maxY = mid.y + zoom * range.y / 2f;

                range = new Vector2( maxX - minX, maxY - minY );
            }
            else
            {
                minX = skin.rangeX[0];  maxX = skin.rangeX[1];
                minY = skin.rangeY[0];  maxY = skin.rangeY[1];

                range = new Vector2( skin.rangeX[1] - skin.rangeX[0] , skin.rangeY[1] - skin.rangeY[0] );
            }


            if( skin.gridSize > 0 )
            {   
                for ( var x = position.x ; x < position.xMax; x += skin.gridSize)
                    
                    RenderLine( x, position.y, x, position.yMax, skin.gridColor , 1.5f );

                for (var y = position.y ; y < position.yMax; y += skin.gridSize)
                    
                    RenderLine( position.x, y, position.xMax, y, skin.gridColor , 1.5f );
            }

            
            if( func == null ) return;

            if( range.x < 0 || range.x < 0 ) return;

            if( skin.drawAxis )
            {
                var lx = ( 0 - minX) / range.x ;
                var ly = 1 - ( 0 - minY) / range.y ;

                lx = position.x + lx * position.width;
                ly = position.y + ly * position.height;
                
                if ( lx >= position.x && lx <= position.xMax )
                    RenderLine ( lx, position.y, lx, position.yMax, isDark ? Color.white : Color.black );
                if ( ly >= position.y && ly <= position.yMax )
                    RenderLine ( position.x, ly, position.xMax, ly, isDark ? Color.white : Color.black );
            }

            var y0 = func( 0 );

            var step = range.x / position.width;

            for( float x1 = minX + step ; x1 < maxX - step ; x1 += step )
            {
                var y1 = func( x1 );

                var x0 = x1 - step;

                var lx1 = ( x0 - minX ) / range.x ;
                var lx2 = ( x1 - minX ) / range.x ;

                var ly1 = 1 - ( y0 - minY ) / range.y ;
                var ly2 = 1 - ( y1 - minY ) / range.y ;

                RenderLine
                (
                    position.x + lx1 * position.width,
                    position.y + ly1 * position.height,
                    position.x + lx2 * position.width,
                    position.y + ly2 * position.height, 

                    skin.lineColor,
                    skin.lineThickness
                );

                y0 = y1;
            }


            if( needsRepaint ) { // https://answers.unity.com/questions/505697/how-to-repaint-from-a-property-drawer.html
                foreach (var item in ActiveEditorTracker.sharedTracker.activeEditors)
                    if (item.serializedObject == property.serializedObject) { item.Repaint(); return; }
            }
        }
    }

#endif
}

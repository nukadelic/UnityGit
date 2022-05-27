using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Nukadelic.Utils;

[DisallowMultipleComponent]
public class AlignChildrenFlat : MonoBehaviour
{
    public enum Up { X, Y , Z }

    public Up up = Up.Y;

    public Vector3 GetDirection()
    {
        switch(up) {
            case Up.X: return Vector3.right;
            case Up.Y: return Vector3.up;
        }   /* Up.Z */ return Vector3.forward;
    }

    public int rows = 1;

    public float spacing = 0.1f;
    
    //public bool centerLastRow = false;

    public bool sortBySize = false;

    public bool excludeDisabled = true;

    private void OnValidate()
    {
        rows = Mathf.Max( 1 , rows );
    }

    struct IV { public int i ; public float v; }

    List<IV> volume;
    List<Transform> kids;
    List<int> indexes;

    public void Align()
    {
        kids = transform.Children( excludeDisabled );

        //Debug.Log( "Count " + kids.Count );

        int row_item_count = Mathf.Min( kids.Count ,  Mathf.Max( 1, Mathf.CeilToInt( kids.Count / rows ) ) );

        if( row_item_count > kids.Count )
        {
            rows = 1;

            row_item_count = kids.Count;
        }    
        
        //Debug.Log( "Raw item count: " + row_item_count );

        indexes = new List<int>();

        for( var i = 0; i < kids.Count; ++i)
        {
            //Debug.Log( $"{i} {kids[i].name} c={kids[i].GetColliderBounds().extents} m={kids[i].GetMeshBounds().extents}", kids[i] );

            indexes.Add( i );
        }

        if ( sortBySize )
        {
            volume = indexes.Select( x => new IV{ i = x , v = kids[ x ]?.GetMeshVolume() ?? 0 } ).ToList( );
            
            volume.Sort( (a, b) => a.v.CompareTo( b.v ) );

            //Debug.Log( "Vol: " + string.Join( ", " , volume.Select( x => $"{x.i}={x.v}" ) ) );

            indexes = volume.Select( x => x.i ).ToList( );
        }
        
        // Debug.Log( "Idx: " + string.Join(", ", indexes ) );

        for ( var i = 0; i < kids.Count; ++i )
        {
            int row_i = Mathf.FloorToInt( i / ( float ) row_item_count );

            int i_x = i % row_item_count;

            int count = row_item_count;

            //var needs_center = centerLastRow && row_i == rows + 1 && rows > 1;

            //if ( needs_center )
            //{
            //    count = kids.Count - ( rows ) * row_item_count;

            //    i_x = count - ( kids.Count - j );
            //}

            float x = ( i_x - count / 2f ) * spacing;

            //if( needs_center ) x += ( count % 2 == 0 ? 1 : -1/2 ) * spacing / 2f;

            float y = ( row_i - rows / 2f ) * spacing;

            Vector3 p = Vector3.zero;

            switch( up )
            {
                case Up.Y: p = new Vector3( x, 0, y ); break;
                case Up.X: p = new Vector3( 0, x, y ); break;
                case Up.Z: p = new Vector3( x, y, 0 ); break;
            }

            kids[ indexes[ i ] ].transform.localPosition = p;
        }
    }
    
    public bool gizmos = false;

    public void OnDrawGizmosSelected()
    {
        if( ! gizmos || kids == null || kids.Count < 1 ) return;

        var d = GetDirection();

        int i = 0;

        foreach( var c in kids )
        {
            var s = $"{ i } : { indexes[ i ] }\n{ c.GetMeshVolume( ).ToString( "N3" ) }";

            i ++ ;

            #if UNITY_EDITOR
                Handles.Label( c.position + d * spacing, s );
            #endif
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(AlignChildrenFlat))]
    class Inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space( 10 );

            var script = ( AlignChildrenFlat ) target;

            if( GUILayout.Button( "Align" , GUILayout.Height( 22 ) ) ) script.Align();
        }
    }
    #endif
}


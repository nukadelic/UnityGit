namespace Nukadelic.Processors
{
    // WIP .. multi material isn't rendereing for some reason 

    using System.Collections.Generic;
    using UnityEngine;
    #if UNITY_EDITOR
    using System.IO;
    using UnityEditor;
    #endif

    [CreateAssetMenu(fileName = "_SaveIcon", menuName = "SO/SaveIconProfile")]
    public class SaveIconProfile : ScriptableObject
    {
        public string prefix = "food_";

        public Vector3 offset = Vector3.zero;
        public Vector3 rotate = Vector3.zero;

        public float scale = 1;

        public string colorKeyword = "_Color";

        //public bool preview = false;
    }


    #if UNITY_EDITOR

    [CustomEditor(typeof(SaveIconProfile))]
    public class SaveIconProfileEditor : Editor
    {
        int preview_index = 0;

        public override void OnInspectorGUI()
        {
            var profile = target as SaveIconProfile;

            if ( GUILayout.Button("Generate Preview Icons") )
            {
                Scan();
            }

            GUILayout.Space( 10 );

            base.OnInspectorGUI();

            GUILayout.Space( 12 );

            if( true ) // profile.preview )
            {
                var prefabs = GetPrefabs();

                preview_index = ( int ) GUILayout.HorizontalSlider( preview_index , 0, prefabs.Count - 1 );

                // GUILayout.Space(128);

                GUILayout.Space( 8 );

                GUILayout.Box( "" , GUILayout.Height( 128 ) , GUILayout.ExpandWidth( true ) );

                var rect = GUILayoutUtility.GetLastRect();

                if ( rect.width > rect.height )
                {
                    rect.x = (rect.width - rect.height) / 2f;

                    rect.width = rect.height;
                }
                else
                {
                    rect.height = rect.width;
                }

                var size = ( int ) Mathf.Min( rect.width, rect.height );

                if ( size > 0 && TryOpenPrefab( prefabs[ preview_index ], out GameObject prefab ) )
                {
                    var texture = Render( prefab, size );

                    GUI.DrawTexture( rect, texture, ScaleMode.StretchToFill, false );

                    DestroyImmediate( texture );

                }
            }
        }

        Texture2D last;

        List<string> GetPrefabs()
        {
            List<string> prefabs = new List<string>();

            var profile = target as SaveIconProfile;

            var profile_path = AssetDatabase.GetAssetPath( profile );

            var folder = Directory.GetParent( profile_path );

            foreach ( var file_path in Directory.GetFiles( folder.ToString() ) )
        
                if ( file_path.EndsWith(".prefab") ) 
                
                    prefabs.Add( file_path.Substring( Application.dataPath.Length - 6 ) );

            return prefabs;
        }

        bool TryOpenPrefab( string prefab_path , out GameObject prefab )
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>( prefab_path );

            if( prefab == null ) prefab = ( GameObject ) AssetDatabase.LoadMainAssetAtPath( prefab_path );

            if ( prefab != null ) return true;
        
            Debug.LogWarning($"Failed to open: { prefab_path } skipping...");

            return false;
        }

        List<T> GetComponents<T>( GameObject prefab )
        {
            List<T> list = new List<T>();
            list.AddRange(prefab.GetComponentsInChildren<T>());
            list.AddRange(prefab.GetComponents<T>());
            return list;
        }

        Texture2D Render( GameObject prefab , int texture_size = 128 )
        {
            // return AssetPreview.GetAssetPreview( prefab ) as Texture2D;

            var profile = target as SaveIconProfile;

            var preview = new PreviewRenderUtility(false);
        
            preview.BeginStaticPreview( new Rect( 0, 0, texture_size, texture_size ) );

            preview.camera.transform.position = -Vector3.forward * 10f;
            preview.camera.backgroundColor = new Color(0, 0, 0, 0);
            preview.camera.transform.LookAt(Vector3.zero, Vector3.up);
            preview.camera.clearFlags = CameraClearFlags.SolidColor;
            preview.camera.farClipPlane = 100;

            //float halfSize = Mathf.Max(bounds.extents.magnitude, 0.0001f);
            //float distance = halfSize * 3.8f;

            //Quaternion rot = Quaternion.Euler(-m_PreviewDir.y, -m_PreviewDir.x, 0);
            //Vector3 pos = bounds.center - rot * (Vector3.forward * distance);

            //previewData.renderUtility.camera.transform.position = pos;
            //previewData.renderUtility.camera.transform.rotation = rot;
            //previewData.renderUtility.camera.nearClipPlane = distance - halfSize * 1.1f;
            //previewData.renderUtility.camera.farClipPlane = distance + halfSize * 1.1f;

            preview.lights[0].intensity = .7f;
            preview.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0);
            preview.lights[1].intensity = .7f;
            preview.lights[1].transform.rotation = Quaternion.Euler(340, 218, 177);

            preview.ambientColor = new Color(.1f, .1f, .1f, 0);

            //TODO : fix preview - now a default cube prefab will render, however the food items won't 
            // some reading materials : 
            // https://github.com/CyberFoxHax/Unity3D_PreviewRenderUtility_Documentation/wiki/PreviewRenderUtility.AddSingleGO
            // https://www.videopoetics.com/tutorials/capturing-stereoscopic-panoramas-unity/
            // https://answers.unity.com/questions/1332916/any-idea-how-to-use-previewrenderutility.html
            // https://github.com/Unity-Technologies/UnityCsReference/blob/c84064be69f20dcf21ebe4a7bbc176d48e2f289c/Editor/Mono/Inspector/GameObjectInspector.cs
            // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/PreviewRenderUtility.cs

            prefab.transform.position = Vector3.zero;

            var bounds = new Bounds();

            List<MeshRenderer> mesh_renderers = new List<MeshRenderer>();
            mesh_renderers.AddRange( prefab.GetComponentsInChildren<MeshRenderer>() );
            mesh_renderers.AddRange( prefab.GetComponents<MeshRenderer>() );
            foreach ( var mesh_renderer in mesh_renderers )

                bounds.Encapsulate(mesh_renderer.bounds);

            var bounds_max = Mathf.Max( bounds.size.x, bounds.size.y, bounds.size.z );

            var mesh_scale = Vector3.one * ( 1 / bounds_max ) * profile.scale;

            var mesh_rotation = Quaternion.Euler( profile.rotate );

            var mesh_matrix = Matrix4x4.TRS( profile.offset * profile.scale, mesh_rotation , mesh_scale );


            List<MeshFilter> mesh_filters = new List<MeshFilter>();
            mesh_filters.AddRange(prefab.GetComponentsInChildren<MeshFilter>());
            mesh_filters.AddRange(prefab.GetComponents<MeshFilter>());
            foreach ( var mesh_filter in mesh_filters )
            {
                var mesh = mesh_filter.sharedMesh;

                var mesh_render = mesh_filter.GetComponent<MeshRenderer>();

                foreach ( var mesh_mat in mesh_render.sharedMaterials )
                {
                    MaterialPropertyBlock block = new MaterialPropertyBlock();

                    block.SetColor( profile.colorKeyword, mesh_mat.color );

                    for (var i = 0; i < mesh.subMeshCount; ++i)

                        preview.DrawMesh( mesh, mesh_matrix, mesh_mat, i, block );
                }

            }

            //preview.camera.Render();

            preview.Render( true );

            //preview.Render( false );

            var texture = preview.EndStaticPreview();

            preview.Cleanup();

            return texture;
        }

        void Scan()
        {
            var profile = target as SaveIconProfile;

            var profile_path = AssetDatabase.GetAssetPath( profile );

            var profile_file_name = Path.GetFileName( profile_path );

            var assets_path = profile_path.Substring( 0, profile_path.IndexOf( profile_file_name ) );

            var folder = Directory.GetParent( profile_path );

            var folder_icons = Path.Combine( folder.ToString() , "Icons" );

            if ( ! Directory.Exists( folder_icons ) ) Directory.CreateDirectory( folder_icons );



            foreach ( var file_path in Directory.GetFiles( folder.ToString() ) )
            {
                if ( ! file_path.EndsWith(".prefab") ) continue;
            
                var prefab_name = Path.GetFileName( file_path );
            
                var prefab_path = Path.Combine( assets_path, prefab_name );

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>( prefab_path );

                if( prefab == null )
                {
                    prefab = ( GameObject ) AssetDatabase.LoadMainAssetAtPath( prefab_path );
                }

                if ( prefab == null )
                {
                    Debug.LogWarning($"Failed to open: {prefab_path} skipping...");

                    continue;
                }

                var icon_name = string.Empty;
            
                if( prefab_name.StartsWith( profile.prefix ) ) 

                    icon_name = profile.prefix + "icon_" + prefab_name.Replace( profile.prefix , "" ).Replace(".prefab", "") + ".png"; 
                else
                    icon_name  = profile.prefix + "icon_" + prefab_name.Replace(".prefab","" ) + ".png";
        
                var texture = Render(prefab, 128); 
            
                //var render_texture = preview.EndPreview() as RenderTexture;
                //texture = new Texture2D(256, 256, render_texture.graphicsFormat, 1, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
                //texture.SetPixels(0, 0, 256, 256, colors, 0);
                //texture.Apply(false);
                //try { Graphics.CopyTexture(preview.EndPreview(), texture); }
                //catch (System.Exception e) { Debug.LogError(e); }
                //finally { preview.Cleanup(); }
                //texture.Apply(false);

                //if( last != null ) DestroyImmediate( last ); 
            
                // DestroyImmediate( prefab , true );

                // reimport 
                // AssetDatabase.ImportAsset( prefab_path , ImportAssetOptions.ForceUpdate );

                last = texture;

                // if ( profile.preview ) break;

                byte[] bytes = last.EncodeToPNG();
            
                File.WriteAllBytes( Path.Combine( folder_icons , icon_name ) , bytes);
            }
        }
    }

    #endif
}

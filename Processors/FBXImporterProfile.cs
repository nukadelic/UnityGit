namespace Nukadelic.Processors
{
    using UnityEngine;
    using System.Collections.Generic;

    #if UNITY_EDITOR
    using System.IO;
    using UnityEditor;
    #endif

    [CreateAssetMenu(fileName = "_ImportFBX", menuName = "SO/FBXImporterProfile")]
    public class FBXImporterProfile : ScriptableObject
    {
        public Shader shader;

        public string ColorKeyword = "_BaseColor";

        public float transformScale = 10f;

        [Header("Files")]
        public string prefix = "group_";
        //public bool moveFbxToFolder = true;

        [Header("Edit FBX")]
        public bool removeMaterials = true;
    }

    #if UNITY_EDITOR

    [CustomEditor(typeof(FBXImporterProfile))]
    public class FBXImporterProfileEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if( GUILayout.Button("Scan folder") )
            {
                Scan();
            }

            base.OnInspectorGUI();
        }

        void Scan()
        {
            var profile = target as FBXImporterProfile;

            var profile_path = AssetDatabase.GetAssetPath( profile );

            var profile_file_name = Path.GetFileName( profile_path );

            var assets_path = profile_path.Substring( 0, profile_path.IndexOf( profile_file_name ) );

            var folder = Directory.GetParent( profile_path );

            var folder_materials = Path.Combine( folder.ToString() , "Materials" );

            if( ! Directory.Exists( folder_materials ) ) Directory.CreateDirectory( folder_materials );

            var folder_prefabs = Path.Combine( folder.ToString() , "Prefabs" );

            if( ! Directory.Exists( folder_prefabs ) ) Directory.CreateDirectory( folder_prefabs );

            float prefab_count = 0f;

            foreach( var file_path in Directory.GetFiles( folder.ToString() ) )
            {
                if( EditorUtility.DisplayCancelableProgressBar( "FBX Importer", file_path,  0.5f ) ) break;

                if ( file_path.EndsWith( ".fbx" ) )
                {
                    var fbx_name = Path.GetFileName( file_path );

                    var fbx_path = Path.Combine( assets_path, fbx_name );

                    var fbx_prefix = profile.prefix + "fbx_";

                    if ( ! string.IsNullOrWhiteSpace(fbx_prefix) && ! fbx_name.StartsWith(fbx_prefix) )
                    {
                        var new_fbx_path = fbx_path.Replace( fbx_name , fbx_prefix + fbx_name );

                        AssetDatabase.RenameAsset( fbx_path, fbx_prefix + fbx_name );

                        fbx_path = new_fbx_path;
                    }

                
                    //if (profile.moveFbxToFolder)
                    //{
                    //    var fbx_folder = Path.Combine(folder.ToString(), "FBX");

                    //    if ( ! Directory.Exists(fbx_folder) ) 
                    //    {
                    //        AssetDatabase.CreateFolder( assets_path , "FBX" );
                    //    }

                    //    var new_fbx_path = assets_path + "FBX/" + Path.GetFileName(fbx_path);

                    //    var error = AssetDatabase.MoveAsset(fbx_path, new_fbx_path);

                    //    if ( ! string.IsNullOrEmpty(error)) Debug.LogError(error);

                    //    else fbx_path = new_fbx_path;
                    //}

                    var fbx = AssetDatabase.LoadAssetAtPath<GameObject>( fbx_path );

                    var prefab_name = fbx.name.Replace( fbx_prefix , profile.prefix + "_" ) + ".prefab";

                    if ( File.Exists( Path.Combine( folder_prefabs , prefab_name ) ) )
                    {
                        Debug.Log( fbx.name + " Prefab already exists, skipping");
                        continue;
                    }

                    List<Material> materials = new List<Material>();

                    foreach( var m in GetMaterials(fbx_path) )
                    {
                        var m_name = profile.prefix + "m_" + m.name + ".mat";
                    
                        var m_asset_path = assets_path + "Materials/" + m_name;

                        Material material;

                        if ( ! File.Exists( Path.Combine( folder_materials , m_name ) ) )
                        {
                            material = new Material( profile.shader );

                            material.SetColor( profile.ColorKeyword , m.color );
                    
                            AssetDatabase.CreateAsset( material, m_asset_path );
                        }
                        else
                        {
                            material = AssetDatabase.LoadAssetAtPath<Material>( m_asset_path );

                            bool save = false;

                            if( material.shader != profile.shader )     { material.shader = profile.shader;     save = true; }
                            if( material.color != m.color )             { material.color = m.color;             save = true; }

                            if( save )
                            {
                                AssetDatabase.SaveAssetIfDirty(AssetDatabase.GUIDFromAssetPath(m_asset_path));
                            }
                        }

                        materials.Add( material );

                    }

                    ModelImporter importer = AssetImporter.GetAtPath(fbx_path) as ModelImporter;

                    bool reimport = false;

                    if (profile.removeMaterials)
                    {
                        importer.materialImportMode = ModelImporterMaterialImportMode.None;

                        reimport = true;
                    }

                    if (reimport)
                    {
                        AssetDatabase.ImportAsset( fbx_path, ImportAssetOptions.ForceUpdate );
                    
                        fbx = AssetDatabase.LoadAssetAtPath<GameObject>( fbx_path );
                    }





                    var fbx_instance = PrefabUtility.InstantiatePrefab( fbx ) as GameObject;

                    fbx_instance.GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();

                    // foreach( var m in materials ) DestroyImmediate( m );

                    fbx_instance.transform.localScale = Vector3.one * profile.transformScale;

                    var prefab_path = assets_path + "Prefabs/" + prefab_name;

                    PrefabUtility.SaveAsPrefabAsset( fbx_instance , prefab_path );

                    prefab_count ++ ;

                    DestroyImmediate( fbx_instance );

                }
            }

            EditorUtility.ClearProgressBar();

            Debug.Log( "Created " + prefab_count + " prefabs" );

            //  AssetDatabase.Refresh();
        }

        List<MaterialProp> GetMaterials( string fbx_path )
        {
            List<MaterialProp> materialProps = new List<MaterialProp>();

            //AssetDatabase.ExtractAsset( subAsset, destinationPath );

            //var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbx_path);
            //var fbx_materials = fbx.GetComponent<MeshRenderer>().sharedMaterials;
            //foreach (var mat in fbx_materials) Debug.Log(mat.name + " " + mat.color);

            var fbx_sub_assets = AssetDatabase.LoadAllAssetsAtPath( fbx_path );

            foreach (var sub_asset in fbx_sub_assets)
            {
                if ( ! ( sub_asset is Material ) ) continue;
            
                var m = (Material) sub_asset;

                materialProps.Add( new MaterialProp 
                { 
                    name = m.name , 
                    color = m.color 
                } );
            }

            return materialProps;
        }

        struct MaterialProp
        {
            public string name;
            public Color color;
        }
    }

    #endif

}

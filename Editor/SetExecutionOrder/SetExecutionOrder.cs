#if UNITY_EDITOR

    static System.Type MyScriptType = typeof( ExecutionOrderMono );

    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    
    [UnityEditor.InitializeOnLoadMethod] static void RegisterExecutionOrder()
    {
        // UnityEditor.MonoImporter.SetExecutionOrder( UnityEditor.MonoScript.FromMonoBehaviour(_instance), 10100 );
        
        var all_scripts = UnityEditor.MonoImporter.GetAllRuntimeMonoScripts();

        UnityEditor.MonoScript target = null;

        foreach( var script in all_scripts )
        {
            if( script.GetClass() == MyScriptType )
            {
                target = script;
                break;
            }
        }

        if( target == null ) return;

        var order = UnityEditor.MonoImporter.GetExecutionOrder( target );

        if( order != 10100 ) // Unity.Entities proxy is at 10000 , make sure we execute after that 
        {
            Debug.Log( MyScriptType.Name + " execution order priority set to 10100 , old value = " + order );
            
            UnityEditor.MonoImporter.SetExecutionOrder( target , 10100 );
        }
    }
    
#endif

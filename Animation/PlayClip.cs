using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

/*
    Standalone animation clip player without the need to create animation controller asset 

    works with 2d sprites and 3d skinned mesh renderers 
*/

[ExecuteAlways] public class PlayClip : MonoBehaviour
{
    // -------------------------------------------------------

    public void Pause() { this.enabled = false; }

    public void Play() { this.enabled = true; }

    public void SetClip( AnimationClip clip )
    {
        m_Clip = clip;

        Validate();
    }

    public void SetSpeed( float speed )
    {
        m_Speed = speed;

        Validate();
    }

    public void LastFrame()
    {
        playable.GetAnimationClip().SampleAnimation( gameObject, duration );

        graph.Stop();
    }
    
    public event System.Action<PlayClip> OnClipLoop;

    // -------------------------------------------------------
    
    public bool isPlaying { private set; get; } = false;
    public bool isPaused { private set; get; } = false;
    public float duration { private set; get; } = 0f;
    public int loopCount { private set; get; } = 0;
    public float clipProgress { private set; get; } = 0; // 0 to 1 

    // -------------------------------------------------------


    [SerializeField]        AnimationClip m_Clip;

    [SerializeField]
    [Range(0f,2.5f)]        float m_Speed = 1f;

    [Space(10)]
    
    public UnityEngine.Events.UnityEvent<PlayClip> EventClipLoop;

    // -------------------------------------------------------

    [System.NonSerialized]  AnimationClip prev;
    [System.NonSerialized]  Animator animator;
    [System.NonSerialized]  PlayableGraph graph;
    [System.NonSerialized]  AnimationClipPlayable playable;

    // -------------------------------------------------------
    
    // bool hasPlayable => ! playable.Equals( default( AnimationClipPlayable ) );

    // -------------------------------------------------------

    [System.Serializable]
    public struct Properties
    {
        public bool showAnimator;
    }

    // -------------------------------------------------------

    private void Update()
    {
        if( ! isPlaying || isPaused ) return;

        var anim_time = (float) ( playable.GetTime() / duration );

        clipProgress = anim_time % 1;

        if ( graph.IsPlaying() && anim_time >= ( 1 + loopCount ) )
        {
            loopCount ++ ;

            if( Application.isPlaying )
            {
                OnClipLoop?.Invoke( this );
            }
        }
    }

    void OnEnable()
    {
        #if UNITY_EDITOR
        if( ! Application.isPlaying )
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaved += OnSaved;
        #endif
        
        AttachAnimator();

        Validate();

        if( m_Clip != null )
        {
            if( isPaused && isPlaying )
            {
                isPaused = false;

                playable.Play();

                return;
            }

            ForcePlay();
        }
    }
    void OnSaved(Scene scene)
    {
        if ( enabled ) ForcePlay();
    }

    void ForcePlay()
    {
        if( m_Clip == null || animator == null ) return;

        Clean();

        playable = AnimationPlayableUtilities.PlayClip(animator, m_Clip, out graph);
        playable.SetSpeed( m_Speed );
        playable.Play();

        loopCount = 0;
        duration = m_Clip.averageDuration / m_Speed;

        isPlaying = true;

        prev = m_Clip;
    }

    void AttachAnimator()
    {
        if ( animator == null && ! gameObject.TryGetComponent( out animator ) )
            animator = gameObject.AddComponent<Animator>();

        animator.runtimeAnimatorController = null;

        animator.hideFlags = HideFlags.HideInInspector;
    }

    void OnValidate()
    {
        if( isActiveAndEnabled )
        {
            AttachAnimator();
            Validate();
        }
    }

    void Validate()
    {
        if (m_Clip == null) prev = null;

        if (m_Clip != null && m_Clip != prev)
        {
            ForcePlay();

            return;
        }

        if ( isPlaying && playable.IsValid() ) 
            
            playable.SetSpeed( m_Speed );

        else ForcePlay();
    }

    private void OnDisable()
    {  
        #if UNITY_EDITOR
        if( ! Application.isPlaying )
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaved -= OnSaved;
        #endif

        if( isPlaying && playable.IsValid() )
        {
            isPaused = true;

            playable.Pause();
        }
    }

    void OnDestroy()
    {
        Clean();

        if ( animator )
        {
            if ( Application.isPlaying ) Destroy( animator );
            
            else animator.hideFlags = HideFlags.None;
        }
    }

    void Clean()
    {
        if ( playable.IsValid() ) playable.Destroy();

        if ( graph.IsValid() ) graph.Destroy();

        isPlaying = false;
    }
}

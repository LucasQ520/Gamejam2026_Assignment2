using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class StartTransitionAnimator : MonoBehaviour
{
    [Header("Scene")]
    public string mainSceneName = "MainScene";

    [Header("Input")]
    public KeyCode startKey = KeyCode.Space;

    [Header("Transition")]
    public Animator animator;
    public string triggerName = "Start";

    [Header("Background Video (optional)")]
    public VideoPlayer bgVideo;
    public bool muteVideoAudio = true;

    private bool started = false;

    void Awake()
    {
        Time.timeScale = 1f;

        if (animator == null) animator = GetComponent<Animator>();

        if (bgVideo != null)
        {
            bgVideo.isLooping = true;
            bgVideo.playOnAwake = true;

            if (muteVideoAudio)
                bgVideo.audioOutputMode = VideoAudioOutputMode.None;

            bgVideo.Play();
        }
    }

    void Update()
    {
        if (started) return;

        if (Input.GetKeyDown(startKey))
        {
            started = true;
            animator.ResetTrigger(triggerName);
            animator.SetTrigger(triggerName);
        }
    }

    // Animation Event 
    public void OnTransitionFinished()
    {
        SceneManager.LoadScene(mainSceneName);
    }
}
using UnityEngine;
using UnityEngine.Video;

public class VideoNearPlaneNoFlash : MonoBehaviour
{
    public VideoPlayer vp;
    public bool muteAudio = true;

    private bool shown = false;

    void Awake()
    {
        if (vp == null) vp = GetComponent<VideoPlayer>();

        Time.timeScale = 1f;

        if (muteAudio)
        {
            vp.audioOutputMode = VideoAudioOutputMode.None;
            vp.EnableAudioTrack(0, false);
        }

        vp.playOnAwake = false;
        vp.isLooping = true;
        vp.waitForFirstFrame = true;

        
        vp.targetCameraAlpha = 0f;

        vp.sendFrameReadyEvents = true;
        vp.frameReady += OnFrameReady;
        vp.prepareCompleted += OnPrepared;
    }

    void Start()
    {
        vp.Prepare();
    }

    void OnPrepared(VideoPlayer _)
    {
        vp.Play();
    }

    void OnFrameReady(VideoPlayer _vp, long frameIdx)
    {
        if (shown) return;
        shown = true;

        
        _vp.targetCameraAlpha = 1f;

        _vp.frameReady -= OnFrameReady;
        _vp.sendFrameReadyEvents = false;
    }
}
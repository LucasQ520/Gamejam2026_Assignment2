using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager I { get; private set; }

    [Header("Optional")]
    public AudioSource audioSource;

    private void Awake()
    {
 
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

    
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.loop = true;  
            audioSource.Play();
        }
    }
}


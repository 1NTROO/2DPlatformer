using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public GameObject audioSrc;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void PlayAudio(AudioClip clip, Vector3 position = default, float startTime = 0f, float endTime = 1f)
    {
        var newAudioSrc = Instantiate(audioSrc, position, Quaternion.identity);
        AudioSource audioSource = newAudioSrc.GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.time = startTime * clip.length;
        float clipLength = (endTime - startTime) * clip.length;
        audioSource.Play();
        audioSource.SetScheduledEndTime(AudioSettings.dspTime + clipLength);
        Destroy(newAudioSrc, clipLength + 0.1f);
    }
}

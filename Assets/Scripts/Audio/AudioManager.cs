using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource bgmSource;

    [SerializeField]
    private AudioSource sfxSource;

    [Header("BGM")]
    [SerializeField]
    private AudioClip defaultBGM;

    [Range(0f, 1f)]
    [SerializeField]
    private float bgmVolume = 0.7f;

    [Header("SFX")]
    [Range(0f, 1f)]
    [SerializeField]
    private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        if (bgmSource != null)
        {
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = bgmVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;
        }
    }

    private void Start()
    {
        if (defaultBGM != null)
        {
            PlayBGM(defaultBGM);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null ||
            bgmSource == null)
        {
            return;
        }

        if (bgmSource.clip == clip &&
            bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null ||
            sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(
            clip,
            sfxVolume
        );
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume =
            Mathf.Clamp01(volume);

        if (bgmSource != null)
        {
            bgmSource.volume =
                bgmVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume =
            Mathf.Clamp01(volume);

        if (sfxSource != null)
        {
            sfxSource.volume =
                sfxVolume;
        }
    }
}
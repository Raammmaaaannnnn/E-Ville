using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour
{
    private static SoundEffectManager instance;

    
    private static AudioSource audioSource;
    private static AudioSource randomPitchAudioSource;
    private static AudioSource voiceAudioSource;
    private static SoundEffectLibrary soundEffectLibrary;
    [SerializeField] private Slider sfxSlider;
    private static AudioSource bgmAudioSource;         // NEW: Background music
    [SerializeField] private Slider bgmSlider;        // Optional: separate volume control for BGM
    [SerializeField] private AudioClip defaultBGM;   // Optional default BGM




    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            AudioSource[] audioSources = GetComponents<AudioSource>();
            audioSource = audioSources[0];
            randomPitchAudioSource = audioSources[1];
            voiceAudioSource = audioSources[2];

            // Create or assign BGM AudioSource
            if (audioSources.Length > 3)
                bgmAudioSource = audioSources[3];
            else
                bgmAudioSource = gameObject.AddComponent<AudioSource>();

            bgmAudioSource.loop = true; // loop automatically
            bgmAudioSource.playOnAwake = false;
            if (defaultBGM != null)
            {
                bgmAudioSource.clip = defaultBGM;
                bgmAudioSource.Play();
            }

            soundEffectLibrary = GetComponent<SoundEffectLibrary>();

            DontDestroyOnLoad(gameObject);
            DDOLTracker.Register(gameObject); // <- Track this DDOL object
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void Play(string soundName, bool randomPitch = false)
    {
        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);
        if(audioClip != null)
        {
            if(randomPitch)
            {
                randomPitchAudioSource.pitch = Random.Range(0.5f, 1.5f);
                randomPitchAudioSource.PlayOneShot(audioClip);
            }
            else
            {
                audioSource.PlayOneShot(audioClip);
            }
            
        }
        
    }

    public static void PlayVoice(AudioClip audioClip, float pitch = 1f)
    {
        voiceAudioSource.pitch = pitch;
        voiceAudioSource.PlayOneShot(audioClip);
    }
    // Start is called before the first frame update
    void Start()
    {
        sfxSlider.onValueChanged.AddListener(delegate { OnvalueChanged (); });
    }

    public static void SetVolume(float volume)
    {
        audioSource.volume = volume;
        randomPitchAudioSource.volume = volume;
        voiceAudioSource.volume = volume;
        
    } 
    public static void SetBGMVolume(float volume)
    {
        bgmAudioSource.volume = volume; // same slider for simplicity
    }

    public void OnvalueChanged()
    {
        SetVolume(sfxSlider.value);
    }

    // Optional: separate BGM volume
    public void OnBGMValueChanged()
    {
        if (bgmAudioSource != null && bgmSlider != null)
            bgmAudioSource.volume = bgmSlider.value;
    }

    // Optional: Play any BGM clip at runtime
    public static void PlayBGM(AudioClip clip)
    {
        if (bgmAudioSource != null && clip != null)
        {
            bgmAudioSource.clip = clip;
            bgmAudioSource.Play();
        }
    }
}

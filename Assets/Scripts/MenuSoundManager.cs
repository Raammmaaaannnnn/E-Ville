using UnityEngine;
using UnityEngine.UI;

public class MenuSoundManager : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Slider sfxSlider;

    [Header("Audio")]
    [SerializeField] private AudioSource menuAudioSource;

    private const string VolumeKey = "MenuSFXVolume";

    private void Awake()
    {
        if (menuAudioSource == null)
        {
            menuAudioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        // Load saved volume or default to 1
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(OnSliderValueChanged);
            sfxSlider.value = savedVolume;
        }

        menuAudioSource.volume = savedVolume;
    }

    public void OnSliderValueChanged(float value)
    {
        menuAudioSource.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Play a short menu sound (button click, hover, etc.)
    /// </summary>
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && menuAudioSource != null)
            menuAudioSource.PlayOneShot(clip);
    }
}

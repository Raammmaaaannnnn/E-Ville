using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static PlayerIntoxication;

public class DrunkEffectController : MonoBehaviour
{
    public static DrunkEffectController Instance { get; private set; }

    [Header("References")]
    public PlayerController playerController;
    public PlayerIntoxication intoxication;
    public Volume postProcessVolume;

    [Header("Movement Stagger Settings")]
    public float greenStaggerMultiplier = 0.55f;
    public float orangeStaggerMultiplier = 0.40f;
    public float redStaggerMultiplier = 0.32f;

    // Post-processing effects
    private LensDistortion lensDistortion;
    private ChromaticAberration chromaticAberration;
    private DepthOfField depthOfField;

    [Header("Distortion Settings")]
    public float fluctuationSpeed = 0.6f; // speed of distortion oscillation

    private bool isEffectActive = false;
    private bool isRedLocked = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out lensDistortion);
            postProcessVolume.profile.TryGet(out chromaticAberration);
            postProcessVolume.profile.TryGet(out depthOfField);
        }
    }

    private void Update()
    {
        // Live lens distortion fluctuation (if active)
        if (isEffectActive && lensDistortion != null)
        {
            float range = 0f;
            switch (intoxication.currentLevel)
            {
                case PlayerIntoxication.IntoxicationLevel.Green: range = 0.2f; break;
                case PlayerIntoxication.IntoxicationLevel.Orange: range = 0.4f; break;
                case PlayerIntoxication.IntoxicationLevel.Red: range = 0.6f; break;
            }

            // oscillate between -range and +range smoothly using sine wave
            lensDistortion.intensity.value = Mathf.Sin(Time.time * fluctuationSpeed) * range;
        }
    }

    public void ApplyDrunkEffect()
    {
        if (isRedLocked) return;
        intoxication.DrinkAlcohol();
        StartCoroutine(HandleDrunkEffect());
    }

    
    private IEnumerator HandleDrunkEffect()
    {
        PlayerIntoxication.IntoxicationLevel level = intoxication.currentLevel;

        isEffectActive = true;

        switch (level)
        {
            case PlayerIntoxication.IntoxicationLevel.Green:
                Debug.Log("Player drunk: GREEN");
                ApplyVisualEffects(0.3f, 90f);
                ApplyMovementMultiplier(greenStaggerMultiplier);
                
                break;

            case PlayerIntoxication.IntoxicationLevel.Orange:
                Debug.Log("Player drunk: ORANGE");
                ApplyVisualEffects(0.6f, 100f);
                ApplyMovementMultiplier(orangeStaggerMultiplier);
                break;

            case PlayerIntoxication.IntoxicationLevel.Red:
                Debug.Log("Player drunk: RED - Max Intoxication Reached");
                ApplyVisualEffects(1f, 120f);
                ApplyMovementMultiplier(redStaggerMultiplier);
                isRedLocked = true; // Disable key C permanently
                break;
        }

        yield return null;
    }


    private void ApplyVisualEffects(float chromaticIntensity, float focalLength)
    {
        if (chromaticAberration != null)
            chromaticAberration.intensity.value = chromaticIntensity;

        if (depthOfField != null)
        {
            depthOfField.mode.value = DepthOfFieldMode.Bokeh;
            depthOfField.focalLength.value = focalLength;
        }
    }

    private void ApplyMovementMultiplier(float multiplier)
    {
        if (playerController != null)
            playerController.moveSpeed *= multiplier;
    }

    public void ResetEffects()
    {
        isEffectActive = false;
        if (playerController != null) playerController.moveSpeed = 0.7f; // reset to default moveSpeed
        if (lensDistortion != null) lensDistortion.intensity.value = 0f;
        if (chromaticAberration != null) chromaticAberration.intensity.value = 0f;
        if (depthOfField != null) depthOfField.focalLength.value = 90f;
    }
}

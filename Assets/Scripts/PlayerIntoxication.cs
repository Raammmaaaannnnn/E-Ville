using System.Collections;
using UnityEngine;

public class PlayerIntoxication : MonoBehaviour
{
    public enum IntoxicationLevel { None, Green, Orange, Red }

    [Header("Intoxication Settings")]
    public IntoxicationLevel currentLevel = IntoxicationLevel.None;
    public int intoxicationPoints = 0;
    public int maxGreen = 2;
    public int maxOrange = 4;
    public bool isRedPermanent = false;

    [Header("Decay Durations (seconds)")]
    public float greenDecayTime = 20f;
    public float orangeDecayTime = 40f;

    private Coroutine decayCoroutine;

    private void Start()
    {
        if (IntoxicationUIController.Instance != null)
            IntoxicationUIController.Instance.UpdateIntoxicationUI(currentLevel, 0f);
    }

    public void DrinkAlcohol()
    {
        if (isRedPermanent)
        {
            Debug.Log("Already at max intoxication (Red)!");
            return;
        }

        intoxicationPoints++;
        UpdateIntoxicationLevel();


        // Reset decay timer when new drink consumed
        if (decayCoroutine != null)
            StopCoroutine(decayCoroutine);

        decayCoroutine = StartCoroutine(DecayIntoxication());
    }

    private void UpdateIntoxicationLevel()
    {
        if (intoxicationPoints >= maxOrange)
        {
            currentLevel = IntoxicationLevel.Red;
            isRedPermanent = true;
        }
        else if (intoxicationPoints >= maxGreen)
        {
            currentLevel = IntoxicationLevel.Orange;
        }
        else if (intoxicationPoints > 0)
        {
            currentLevel = IntoxicationLevel.Green;
        }
        else
        {
            currentLevel = IntoxicationLevel.None;
            DrunkEffectController.Instance.ResetEffects();
        }

        if (IntoxicationUIController.Instance != null)
        {
            float progress = Mathf.Clamp01(intoxicationPoints / 3f); // 0–1 range
            IntoxicationUIController.Instance.UpdateIntoxicationUI(currentLevel, progress);
        }
        Debug.Log($"Level → {currentLevel}, Points → {intoxicationPoints}");
    }


    private IEnumerator DecayIntoxication()
    {
        while (!isRedPermanent)
        {
            float waitTime = currentLevel switch
            {
                IntoxicationLevel.Green => greenDecayTime,
                IntoxicationLevel.Orange => orangeDecayTime,
                _ => 0
            };

            if (waitTime <= 0) yield break;
            yield return new WaitForSeconds(waitTime);

            // Decay logic
            if (currentLevel == IntoxicationLevel.Orange)
            {
                intoxicationPoints = maxGreen - 1; // go back to green
            }
            else if (currentLevel == IntoxicationLevel.Green)
            {
                intoxicationPoints = 0; // sober up
            }

            UpdateIntoxicationLevel();

            // If sober, stop coroutine
            if (currentLevel == IntoxicationLevel.None)
                yield break;
        }
    }
}

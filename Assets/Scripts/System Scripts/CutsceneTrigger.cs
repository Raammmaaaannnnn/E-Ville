using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    [Header("Cutscene Settings")]
    public string cutsceneName; // e.g. "Intro", "EnterPub", "DrinkScene", "Ending1"

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered) return;
        if (collision.CompareTag("Player"))
        {
            hasTriggered = true;
            TriggerCutscene();
        }
    }

    private void TriggerCutscene()
    {
        Debug.Log($"Cutscene triggered: {cutsceneName}");

        // 🔹 Placeholder: Replace this with actual cutscene logic later
        // For now, we’ll just simulate a short pause or call a method
        switch (cutsceneName)
        {
            case "Intro":
                // Placeholder
                Debug.Log("Intro cutscene would play here.");
                break;

            case "EnterPub":
                // Placeholder
                Debug.Log("Enter Pub cutscene would play here.");
                break;

            case "DrinkScene":
                // Placeholder
                Debug.Log("Drinking cutscene would play here.");
                break;

            case "Rest":
                // Placeholder
                Debug.Log("Rest cutscene would play here.");
                break;

            case "GoHome":
                // Placeholder
                Debug.Log("Going home cutscene would play here.");
                break;

            default:
                Debug.Log("Generic cutscene placeholder triggered.");
                break;
        }
    }
}


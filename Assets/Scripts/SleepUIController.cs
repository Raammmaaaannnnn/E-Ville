using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEditor.Rendering.LookDev;

public class SleepUIController : MonoBehaviour
{
    public static SleepUIController Instance { get; private set; }

    [Header("UI Elements")]
    
    public GameObject sleepPanel;          // root panel for sleep UI
    public Transform choiceContainer;      // container for dynamically created buttons
    public GameObject choiceButtonPrefab;  // prefab with Button + TMP_Text

    [Header("Fade Settings")]
    public Image fadePanel;         // full-screen CanvasGroup to fade
    public float fadeDuration = 1f;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
            DDOLTracker.Register(gameObject); // <- Track this DDOL object
        }
        else
        {
            Destroy(gameObject);
        }

        if (sleepPanel != null)
            sleepPanel.SetActive(false);
    }

    public void Show()
    {
        // Only show if active scene is HomeInterior
        if (SceneManager.GetActiveScene().name != "House") return;
        if (sleepPanel != null)
        {
            
            sleepPanel.SetActive(true);
            ClearChoices();

            // Create Sleep button
            CreateChoiceButton("Sleep", OnSleepClicked);
            // Create Main Menu button
            CreateChoiceButton("Main Menu", OnMainMenuClicked);
        }
    }

    public void Hide()
    {
        if (sleepPanel != null)
        {
            
            sleepPanel.SetActive(false);
            ClearChoices();
        }
    }

    private void ClearChoices()
    {
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public GameObject CreateChoiceButton(string choiceText, UnityAction onClick)
    {
        GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
        TMP_Text tmp = choiceButton.GetComponentInChildren<TMP_Text>();
        if (tmp != null) tmp.text = choiceText;

        Button btn = choiceButton.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners(); // clear prefab listeners
            btn.onClick.AddListener(onClick);
        }

        return choiceButton;
    }

    private void OnSleepClicked()
    {
        Debug.Log("Sleep clicked - will perform escape.");
        StartCoroutine(SleepRoutine()); 

    }

    private IEnumerator SleepRoutine()
    {
       

        // Mute BGM
        SoundEffectManager.SetBGMVolume(0f);

        // Fade in (screen turns black)
        yield return StartCoroutine(Fade(0f, 1f));
        // 2. Reset player stats IN DARK
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ResetAfterSleep();
        }

        // Simulated short sleep duration
        yield return new WaitForSeconds(2f);
        // Fade out (screen becomes visible again)
        yield return StartCoroutine(Fade(1f, 0f));

        // Restore BGM
        SoundEffectManager.SetBGMVolume(1f);

    }


    private void OnMainMenuClicked()
    {
        //SaveController saveController = FindObjectOfType<SaveController>();
        //if (saveController != null)
        //    saveController.SaveGame(); // override previous save
        // Destroy all runtime DDOL objects safely
        DestroyAllRuntimeDDOL();
        // Load Main Menu scene
        SceneManager.LoadScene("MenuScene");
    }

    private void DestroyAllRuntimeDDOL()
    {
        foreach (var obj in DDOLTracker.DDOLObjects)
        {
            if (obj != null)
                // obj.SetActive(false);
                Destroy(obj);

        }

        DDOLTracker.DDOLObjects.Clear();
    }


    // ---------------------------
    // FADING
    // ---------------------------
    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float t = 0f;
        Color c = fadePanel.color;

        while (t < fadeDuration)
        {
            float a = Mathf.Lerp(startAlpha, endAlpha, t / fadeDuration);
            c.a = a;
            fadePanel.color = c;

            t += Time.deltaTime;
            yield return null;
        }

        c.a = endAlpha;
        fadePanel.color = c;
    }
}


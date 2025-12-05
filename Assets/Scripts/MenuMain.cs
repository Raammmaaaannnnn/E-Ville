using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class MenuMain : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public Button button;

    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public List<VideoClip> introClips;

    [Header("After Videos")]
    public string nextScene = "TownScene";
    private SaveController saveController;
    private string saveLocation;
    

    private void Awake()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
    }

    public void OnStartClick()
    {
        // Reset pause state
        PauseController.ResetPause();

        // Load game scene
        SceneManager.LoadScene(nextScene);
    } 
    

    public void OnOptionClick()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void OnBackFromOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void OnExitClick()
    {
        SaveController.DeleteSave();  // <- STATIC call (will ALWAYS work)

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}


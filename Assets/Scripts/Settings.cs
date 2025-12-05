using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.IO; // Needed for scene loading

public class Settings : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown graphicsDropdown;


    private void Awake()
    {
        // Ensure only one EventSystem exists
        EventSystem[] systems = FindObjectsOfType<EventSystem>();
        if (systems.Length > 1)
        {
            for (int i = 1; i < systems.Length; i++)
            {
                Destroy(systems[i].gameObject);
            }
        }
    }
    private void Start()
    {
        if (graphicsDropdown == null)
        {
            Debug.LogError("Graphics Dropdown not assigned!");
            return;
        }

        // Populate dropdown with QualitySettings names
        graphicsDropdown.ClearOptions();
        graphicsDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));

        // Set dropdown to current quality level
        graphicsDropdown.value = QualitySettings.GetQualityLevel();
        graphicsDropdown.RefreshShownValue();

        // Add listener for changes
        graphicsDropdown.onValueChanged.AddListener(OnGraphicsDropdownChanged);
    }

    public void OnGraphicsDropdownChanged(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        Debug.Log($"Graphics quality changed to: {QualitySettings.names[index]}");
    }

    // ------------------- GO TO MENU (Restart without saving) -------------------
    public void GoToMenu()
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
            if (obj != null )
                // obj.SetActive(false);
                Destroy(obj);

        }

       DDOLTracker.DDOLObjects.Clear();
    }
}

// ------------------------
// DDOL Tracker Helper
// ------------------------
public static class DDOLTracker
{
    public static List<GameObject> DDOLObjects = new List<GameObject>();

    public static void Register(GameObject obj)
    {
        if (!DDOLObjects.Contains(obj))
            DDOLObjects.Add(obj);
    }
}
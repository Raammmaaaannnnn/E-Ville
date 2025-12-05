using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;


public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public static MenuController Instance;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
            DDOLTracker.Register(gameObject); // <- Track this DDOL object

        }
        else if (Instance != this)
        {
            Destroy(gameObject);

        }

    }


    // Start is called before the first frame update
    void Start()
    {
        menuCanvas.SetActive(false);
    }

    // Update is called once per frame
    //void Update()
    //{
    //    if(Input.GetKeyDown(KeyCode.Tab))
    //    {
    //        if (!menuCanvas.activeSelf && PauseController.IsGamePaused)
    //        {
    //            return;
    //        }
    //        menuCanvas.SetActive(!menuCanvas.activeSelf);
    //        PauseController.SetPause(menuCanvas.activeSelf);
    //    }
    //}

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        bool newState = !menuCanvas.activeSelf;

        menuCanvas.SetActive(newState);

        // Only pause if menu is open.
        PauseController.SetPause(newState);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PubDoor : MonoBehaviour
{
    [SerializeField] string sceneToLoad;    // "PubScene"
    [SerializeField] string spawnPointName; // "PubSpawn"

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(LoadSceneCoroutine());
        }
    }

    IEnumerator LoadSceneCoroutine()
    {
        // Optionally play fade out animation
        yield return new WaitForSeconds(0.2f);

        // Subscribe to sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Load the scene
        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find player and spawn point in the new scene
        GameObject player = GameObject.FindWithTag("Player");
        GameObject spawn = GameObject.Find(spawnPointName);

        if (player != null && spawn != null)
        {
            player.transform.position = spawn.transform.position;
            Debug.Log("Player moved to spawn: " + spawn.name);
        }
        else
        {
            Debug.LogWarning("Player or spawn not found!");
        }

        // Unsubscribe from the event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

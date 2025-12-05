using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeDoor : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;       // e.g. "HomeInterior"
    [SerializeField] private string spawnPointName;    // e.g. "HomeSpawn"
    [SerializeField] private float unloadDelay = 60f;  // unload interior after delay

    public static bool isInsideHome = false;          // global flag for player inside home
    private Coroutine unloadCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (!isInsideHome)
            StartCoroutine(EnterHome(other.gameObject));
        else
            StartCoroutine(ExitHome(other.gameObject));
    }

    private IEnumerator EnterHome(GameObject player)
    {
        yield return new WaitForSeconds(0.2f); // optional fade delay

        // Load interior additively
        if (!SceneManager.GetSceneByName(sceneToLoad).isLoaded)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
            while (!asyncLoad.isDone) yield return null;
        }

        Scene homeScene = SceneManager.GetSceneByName(sceneToLoad);

        // Move player to interior spawn
        GameObject spawn = FindObjectInScene(spawnPointName, homeScene);
        if (spawn != null)
            player.transform.position = spawn.transform.position;

        // Move player into home scene for physics
        SceneManager.MoveGameObjectToScene(player, homeScene);

        // Set home scene as active
        SceneManager.SetActiveScene(homeScene);

        // Update global flag
        isInsideHome = true;

        // Cancel pending unload if any
        if (unloadCoroutine != null)
        {
            StopCoroutine(unloadCoroutine);
            unloadCoroutine = null;
        }

        // Show Sleep UI
        if (SleepUIController.Instance != null)
            SleepUIController.Instance.Show();
        else
            Debug.LogWarning("SleepUIController.Instance not found!");
    }

    private IEnumerator ExitHome(GameObject player)
    {
        yield return new WaitForSeconds(0.2f);

        // Ensure TownScene is loaded before moving player
        if (!SceneManager.GetSceneByName("TownScene").isLoaded)
        {
            yield return SceneManager.LoadSceneAsync("TownScene", LoadSceneMode.Additive);
        }

        // Move player back to Town
        Scene townScene = SceneManager.GetSceneByName("TownScene");
        GameObject spawn = FindObjectInScene("HomeFront", townScene);
        if (spawn != null)
            player.transform.position = spawn.transform.position;

        SceneManager.SetActiveScene(townScene);

        // Update global flag
        isInsideHome = false;

        // Hide Sleep UI immediately
        if (SleepUIController.Instance != null)
            SleepUIController.Instance.Hide();

        // Start delayed unload of home scene
        unloadCoroutine = StartCoroutine(UnloadAfterDelay(unloadDelay));
    }

    private IEnumerator UnloadAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (SceneManager.GetSceneByName(sceneToLoad).isLoaded)
        {
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneToLoad);
            while (!asyncUnload.isDone) yield return null;
            Debug.Log($"Unloaded scene {sceneToLoad} after {delay} seconds");
        }

        unloadCoroutine = null;
    }

    private GameObject FindObjectInScene(string name, Scene scene)
    {
        if (!scene.isLoaded) return null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == name) return root;
        }
        return null;
    }
}

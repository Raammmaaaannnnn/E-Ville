using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PubDoor : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private string spawnPointName;
    [SerializeField] private float unloadDelay = 60f; // how long before unloading pub

    private bool isInsidePub = false;
    private Coroutine unloadCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isInsidePub)
        {
            StartCoroutine(EnterPub(other.gameObject));
        }
        else if (other.CompareTag("Player") && isInsidePub)
        {
            StartCoroutine(ExitPub(other.gameObject));
        }
    }

    private IEnumerator EnterPub(GameObject player)
    {
        yield return new WaitForSeconds(0.2f); // fade out time, optional

        // Load additively instead of replacing
        if (!SceneManager.GetSceneByName(sceneToLoad).isLoaded)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
            while (!asyncLoad.isDone) yield return null;
        }

        // Move player to spawn inside the new scene
        Scene pubScene = SceneManager.GetSceneByName(sceneToLoad);
        GameObject spawn = FindObjectInScene(spawnPointName, pubScene);
        if (spawn != null)
        {
            player.transform.position = spawn.transform.position;
        }

        // Set this as the active scene so lighting/UI works properly
        SceneManager.SetActiveScene(pubScene);

        isInsidePub = true;

        // Cancel any pending unload coroutine
        if (unloadCoroutine != null)
        {
            StopCoroutine(unloadCoroutine);
            unloadCoroutine = null;
        }
    }

    private IEnumerator ExitPub(GameObject player)
    {
        yield return new WaitForSeconds(0.2f);

        // Move player back to Town spawn
        Scene townScene = SceneManager.GetSceneByName("TownScene");
        GameObject spawn = FindObjectInScene("TownSpawn", townScene);
        if (spawn != null)
        {
            player.transform.position = spawn.transform.position;
        }

        // Switch active scene back to Town
        SceneManager.SetActiveScene(townScene);

        isInsidePub = false;

        // Start the delayed unload
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

    // Helper: find objects by name inside a specific scene
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

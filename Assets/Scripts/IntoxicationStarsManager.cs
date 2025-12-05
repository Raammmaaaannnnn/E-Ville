using System.Collections;
using UnityEngine;

public class IntoxicationStarsManager : MonoBehaviour
{
    public static IntoxicationStarsManager Instance { get; private set; }

    public int starCount = 0;

    public delegate void StarsChanged(int stars);
    public event StarsChanged OnStarsChanged;

    [Header("Star Decay Settings")]
    public float baseDecayTime = 10f; // time in seconds for 1 star to decay
    public float decayMultiplierPerStar = 2f; // how much longer for 2nd star


    private Coroutine decayCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
           // DDOLTracker.Register(gameObject); // <- Track this DDOL object
        }
        else
        {
            Destroy(gameObject);
        }
    }

    

    public int GetStars()
    {
        return starCount;
    }
    public void AddStars(int amount)
    {
        starCount += amount;

        if (starCount > 3) starCount = 3;

        OnStarsChanged?.Invoke(starCount);


        // Restart decay whenever stars are added
        if (decayCoroutine != null)
            StopCoroutine(decayCoroutine);

        if (starCount < 3)
            decayCoroutine = StartCoroutine(StarDecayCoroutine());

        // 🔥 START CHAOS MODE
        if (starCount == 3)
            HouseSpawnManager.Instance?.StartChaosMode();
    }

    private IEnumerator StarDecayCoroutine()
    {
        while (starCount > 0 && starCount < 3)
        {
            float decayTime = baseDecayTime;

            // 2 stars decay slower
            if (starCount == 2)
                decayTime *= decayMultiplierPerStar;

            yield return new WaitForSeconds(decayTime);

            starCount--;
            OnStarsChanged?.Invoke(starCount);

            if (starCount < 3 )
                HouseSpawnManager.Instance?.StopChaosMode();
        }
    }

    public void ResetStars()
    {
        starCount = 0;
        OnStarsChanged?.Invoke(starCount);
        if (decayCoroutine != null)
            StopCoroutine(decayCoroutine);
        HouseSpawnManager.Instance?.StopChaosMode();

        // Destroy all attacker NPCs by tag
        GameObject[] attackers = GameObject.FindGameObjectsWithTag("AttackerNPC");
        foreach (var attacker in attackers)
        {
            Destroy(attacker);
        }
        // Also clear the HouseSpawnManager tracking list
        if (HouseSpawnManager.Instance != null)
            HouseSpawnManager.Instance.spawnedAttackers.Clear();
    }
}

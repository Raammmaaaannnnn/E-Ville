using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseSpawnManager : MonoBehaviour
{
    public static HouseSpawnManager Instance;

    [Header("Doors that can spawn attackers")]
    public List<HouseDoor> houseDoors = new List<HouseDoor>();

    [Header("Possible attackers to spawn")]
    public List<GameObject> attackerNPCPrefabs = new List<GameObject>();

    [Header("Spawn Settings")]
    public float spawnInterval = 5f;  // spawn every X seconds
    private bool spawningActive = false;

    // Track all spawned attackers at runtime
    [HideInInspector]
    public List<GameObject> spawnedAttackers = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
            //DDOLTracker.Register(gameObject); // <- Track this DDOL object
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartChaosMode()
    {
        if (spawningActive) return;

        spawningActive = true;
        StartCoroutine(SpawnLoop());
    }
    

    public void StopChaosMode()
    {
        spawningActive = false;

        // Destroy all active attackers when chaos stops
        for (int i = spawnedAttackers.Count - 1; i >= 0; i--)
        {
            if (spawnedAttackers[i] != null)
                Destroy(spawnedAttackers[i]);
           
        }
        spawnedAttackers.Clear();
    }

    private IEnumerator SpawnLoop()
    {
        while (spawningActive)
        {
            yield return new WaitForSeconds(spawnInterval);

            // pick random door
            HouseDoor randomDoor = houseDoors[Random.Range(0, houseDoors.Count)];

            // make it spawn if it's not busy
            if (!randomDoor.isSpawning)
                randomDoor.SpawnAttacker();
        }
    }

    // Helper to register a spawned attacker
    public void RegisterSpawnedAttacker(GameObject attacker)
    {
        if (!spawnedAttackers.Contains(attacker))
            spawnedAttackers.Add(attacker);
    }

    // Helper to unregister attacker on destroy
    public void UnregisterAttacker(GameObject attacker)
    {
        if (spawnedAttackers.Contains(attacker))
            spawnedAttackers.Remove(attacker);
    }
}
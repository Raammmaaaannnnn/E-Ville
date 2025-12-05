using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HouseDoor : MonoBehaviour
{
    public Animator doorAnim;

    public Transform spawnPoint;

    [HideInInspector]
    public bool isSpawning = false;

    private GameObject lastSpawnedType;

    public void SpawnAttacker()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        isSpawning = true;

        // play door animation
        doorAnim.SetTrigger("ShouldOpen");

        yield return new WaitForSeconds(0.3f); // match open animation timing

        // choose NPC type but avoid duplicates
        GameObject chosen = null;
        int tries = 5;

        while (tries > 0)
        {
            GameObject candidate = HouseSpawnManager.Instance.attackerNPCPrefabs[
                Random.Range(0, HouseSpawnManager.Instance.attackerNPCPrefabs.Count)
            ];

            if (candidate != lastSpawnedType)
            {
                chosen = candidate;
                break;
            }

            tries--;
        }

        if (chosen == null)
            chosen = HouseSpawnManager.Instance.attackerNPCPrefabs[0];

        lastSpawnedType = chosen;

        // spawn attacker
        GameObject newAttacker = Instantiate(chosen, spawnPoint.position, Quaternion.identity);

        // set correct tag
        newAttacker.tag = "AttackerNPC";

        // add to runtime list in HouseSpawnManager
        if (!HouseSpawnManager.Instance.spawnedAttackers.Contains(newAttacker))
            HouseSpawnManager.Instance.spawnedAttackers.Add(newAttacker);

        yield return new WaitForSeconds(0.3f);
        isSpawning = false;
    }

}

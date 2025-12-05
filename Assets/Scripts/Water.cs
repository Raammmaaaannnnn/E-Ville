using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    PlayerController player;

    private void Start()
    {
        player = GetComponent<PlayerController>();
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if it's the player
        if (other.CompareTag("Player"))
        {

            if (player != null)
            {
                // Instantly kill the player
                player.TakeDamage((int)player.maxHealth, gameObject);
            }

            return; // stop here, do not destroy player
        }

        // Destroy anything else leaving the water
        Destroy(other.gameObject);
    }
}

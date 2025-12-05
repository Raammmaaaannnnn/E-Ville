using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    private Animator anim;
    public bool destroyed { get; private set; }

    public string DesID { get; private set; }

    public GameObject itemPrefab;

    [Header("Star Settings")]
    [Tooltip("Number of objects that need to be destroyed before awarding 1 star.")]
    public int objectsToDestroyForStar = 3; // configurable
    private static int destroyedCount = 0;   // shared across all destructibles

    private void Awake()
    {
        DesID ??= GlobalHelper.GenerateUniqueID(gameObject);
        anim = GetComponent<Animator>();
    }

    public void Hit()
    {
        if (destroyed) return;

        destroyed = true;

        Debug.Log("Destructible hit! Playing animation...");

        anim.SetTrigger("Destruct");
        

        // Optional: Destroy after animation ends
        Destroy(gameObject, 0.8f); // duration should match your animation length

        // Increase global count
        destroyedCount++;

        // Check if player reached the threshold to award 1 star
        if (destroyedCount >= objectsToDestroyForStar)
        {
            
            if (itemPrefab)
            {
                GameObject droppedItem = Instantiate(itemPrefab, transform.position + (Vector3.down * 0.15f), Quaternion.identity);
                droppedItem.GetComponent<BounceEffect>().StartBounce();
            }
            IntoxicationStarsManager.Instance?.AddStars(1);
            destroyedCount = 0; // reset counter for next star
        }


        SoundEffectManager.Play("Destroy");
    }

    public void SetDestroyed(bool Destroyed)
    {

        Destroyed = destroyed;
    }
}

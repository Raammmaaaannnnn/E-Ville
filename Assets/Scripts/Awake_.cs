using UnityEngine;

public class Awake_ : MonoBehaviour
{
    
    void Awake()
    {
        // Check if there’s already an identical object (same name)
        GameObject[] objs = GameObject.FindGameObjectsWithTag(gameObject.tag);

        if (objs.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}
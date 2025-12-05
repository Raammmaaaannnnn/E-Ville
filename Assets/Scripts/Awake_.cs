using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Awake_ : MonoBehaviour
{
    public static Awake_ Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
            DDOLTracker.Register(gameObject); // <- Track this DDOL object
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    [HideInInspector] public GameObject currentItem { get; set; }

    public bool IsEmpty() => currentItem == null;

    public bool TryStackItem(GameObject newItemObj)
    {
        if (currentItem == null) return false;

        Item existing = currentItem.GetComponent<Item>();
        Item incoming = newItemObj.GetComponent<Item>();

        if (existing.CanStackWith(incoming))
        {
            existing.AddToStack(incoming.quantity);
            Destroy(newItemObj); // merged into existing stack
            return true;
        }
        return false;
    }
}

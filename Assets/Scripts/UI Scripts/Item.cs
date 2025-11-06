using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public int ID;
    public string Name;

    public virtual void UseItem()
    {
        Debug.Log("Using item " + Name);
    }
    public virtual void PickUp()
    {
        Sprite itemIcon = GetComponent<Image>().sprite;
        if(ItemPick_PopupUIController.Instance != null )
        {
            ItemPick_PopupUIController.Instance.ShowItemPickup(Name, itemIcon);
        }
    }
}

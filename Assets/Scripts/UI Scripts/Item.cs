using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public int ID;
    public string Name;
    public int quantity = 1;
    public bool isVariant = false;
    public int variantID = 0; // Only used if isVariant = true
    private TMP_Text quantityText;
    private bool IsAlcoholVariant;

    [HideInInspector] public bool isUIItem = true;

    private void Awake()
    {

        quantityText = GetComponentInChildren<TMP_Text>();
        UpdateQuantityDisplay();
    }

    public void UpdateQuantityDisplay()
    {
        if (quantityText != null)
        {
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
        
    }

    public bool CanStackWith(Item other)
    {
        if (other == null) return false;
        if (ID != other.ID) return false;
        if (isVariant != other.isVariant) return false;
        if (isVariant && variantID != other.variantID) return false;
        return true;
    }

    public void AddToStack(int amount = 1)
    {
        quantity += amount;
        UpdateQuantityDisplay();
    }

    public int RemoveFromStack(int amount = 1)
    {
        int removed = Mathf.Min(amount, quantity);
        quantity -= removed;
        UpdateQuantityDisplay();
        return removed;
    }

    public GameObject CloneItem(int newQuantity)
    {
        GameObject clone = Instantiate(gameObject);
        
        Item cloneItem = clone.GetComponent<Item>();
        cloneItem.quantity = newQuantity;
        cloneItem.UpdateQuantityDisplay();

        // Reset UI components
        clone.transform.localScale = Vector3.one;
        CanvasGroup cg = clone.GetComponent<CanvasGroup>();
        if (cg != null) cg.blocksRaycasts = true;

        return clone;
    }


    public virtual void UseItem()
    {
        // By name check
        string lowerName = Name.ToLower();
        if(lowerName.Contains("cosita") || lowerName.Contains("pina") || lowerName.Contains("blu"))
        {
            IsAlcoholVariant = true;
            
        }
        // By ID or variantID check (for extra safety)
        if ((ID == 4 && variantID == 3) || (ID == 5 && variantID == 4) || (ID == 6 && variantID == 5))
        {
            IsAlcoholVariant = true;
            
        }
        
        if(IsAlcoholVariant)
        {
            Debug.Log("Using alcohol " + Name);
        }
        else
        {
            Debug.Log("Using item " + Name);
        }
        
    }
    public virtual void ShowPopup()
    {
        Sprite itemIcon = GetComponent<Image>().sprite;
        if(ItemPick_PopupUIController.Instance != null )
        {
            ItemPick_PopupUIController.Instance.ShowItemPickup(Name, itemIcon);
        }
    }


    public static implicit operator GameObject(Item v)
    {
        throw new NotImplementedException();
    }
}

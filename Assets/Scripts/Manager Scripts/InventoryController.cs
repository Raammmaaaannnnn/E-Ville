using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;


    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;


    public static InventoryController Instance {  get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        itemDictionary = FindAnyObjectByType<ItemDictionary>();

        //for (int i = 0; i < slotCount; i++)
        //{ 
        //    Slot slot = Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<Slot>(); 
        //    if (i < itemPrefabs.Length) 
        //    { 
        //        GameObject item = Instantiate(itemPrefabs[i], slot.transform); 
        //        item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; 
        //        slot.currentItem = item; 
        //    }
        //}
    }

    public Slot GetFirstEmptySlot()
    {
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null) return slot;
        }
        return null;
    }

    public bool AddItem(GameObject itemPrefab)
    {
        //Item itemtoAdd = itemPrefab.GetComponent<Item>();
        //if(itemtoAdd == null) return false;

        ////check if we have this item in the inventory
        //foreach (Transform slotTransform in inventoryPanel.transform)
        //{
        //    Slot slot = slotTransform.GetComponent<Slot>();
        //    if (slot != null && slot.currentItem != null)
        //    {
        //        Item slotItem = slot.currentItem.GetComponent<Item>();
        //        if (slotItem != null && slotItem.ID == itemtoAdd.ID)
        //        {
        //            //stack
        //            slotItem.AddToStack();
        //            return true;
        //        }
        //    }
        //}


        // look for empty slot
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);
                // Make sure its scale/UI is reset
                newItem.transform.localScale = Vector3.one;
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = newItem;

                // Optional: Make sure quantity display is correct
                Item itemComponent = newItem.GetComponent<Item>();
                if (itemComponent != null)
                    itemComponent.UpdateQuantityDisplay();



                return true;
            }
        }

        Debug.Log("Inventory is full");
        return false;
    }

    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach(Transform slotTransform in inventoryPanel.transform)
        {
            
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                invData.Add(new InventorySaveData 
                { 
                    itemID = item.ID, 
                    slotIndex = slotTransform.GetSiblingIndex(), 
                    //quantity = item.quantity
                });

            }
        }
        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        for(int i = 0; i <slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        foreach(InventorySaveData data in inventorySaveData)
        {
            if(data.slotIndex < slotCount)
            {
                Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>(); ;
                
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);

                if(itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    
                    //Item itemComponent = item.GetComponent<Item>();
                    //if(itemComponent != null && data.quantity > 1)
                    //{
                    //    itemComponent.quantity = data.quantity;
                    //    itemComponent.UpdateQuantityDisplay();
                    //}
                    
                    
                    slot.currentItem = item;

                }
            }
        }
    }

}

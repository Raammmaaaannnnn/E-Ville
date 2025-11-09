using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;


    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;


    public static InventoryController Instance {  get; private set; }

    Dictionary<(int id, int variantID), int> itemsCountCache = new();

    public event Action OnInventoryChanged; //event to notify quest system.

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
        RebuildItemCounts();
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

    public void RebuildItemCounts()
    {
        itemsCountCache.Clear();

        foreach( Transform slotTransform in inventoryPanel.transform )
        {
            Slot slot = slotTransform. GetComponent<Slot>();
            if(slot.currentItem !=null)
            {
                Item item = slot.currentItem.GetComponent<Item>();

                if(item != null )
                {
                    var key = (item.ID, item.isVariant ? item.variantID : 0);
                    itemsCountCache[key] = itemsCountCache.GetValueOrDefault(key, 0) + item.quantity;
                }
            }
        }

        OnInventoryChanged?.Invoke();
    }

    public Dictionary<(int id, int variantID), int> GetItemCounts() => itemsCountCache;

    // 🧰 Helper: total count for an item (variant optional)
    public int GetItemCount(int id, int variantID = 0)
    {
        var key = (id, variantID);
        return itemsCountCache.TryGetValue(key, out int count) ? count : 0;
    }

    // 🔹 Cache helpers
    public void AddToCache(Item item)
    {
        var key = (item.ID, item.isVariant ? item.variantID : 0);
        itemsCountCache[key] = itemsCountCache.GetValueOrDefault(key, 0) + item.quantity;
        OnInventoryChanged?.Invoke();
    }

    public void RemoveFromCache(Item item, int amount)
    {
        var key = (item.ID, item.isVariant ? item.variantID : 0);
        if (!itemsCountCache.ContainsKey(key)) return;

        itemsCountCache[key] -= amount;
        if (itemsCountCache[key] <= 0)
            itemsCountCache.Remove(key);

        OnInventoryChanged?.Invoke();
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
                {
                    itemComponent.UpdateQuantityDisplay();
                    AddToCache(itemComponent);
                }

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

        RebuildItemCounts();
    }


    public void RemoveItemsFromInventory(int itemID, int variantID, int amountToRemove)
    {
        foreach(Transform slotTransform in inventoryPanel.transform)
        {
            if (amountToRemove <= 0) break;

            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot?.currentItem?.GetComponent<Item>() is Item item && item.ID == itemID && item.variantID == variantID)
            {
                int removed = Mathf.Min(amountToRemove, item.quantity);
                item.RemoveFromStack(removed);
                amountToRemove -= removed;

                if(item.quantity ==0)
                {
                    Destroy(slot.currentItem);
                    slot.currentItem = null;
                }
            }
        }

        RebuildItemCounts();
    }

}

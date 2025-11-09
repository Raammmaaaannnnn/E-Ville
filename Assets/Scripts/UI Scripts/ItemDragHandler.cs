
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler /* IPointerClickHandler*/
{
    Transform originalParent;
    CanvasGroup canvasGroup;

    private InventoryController inventoryController;

    //public float minDropDistance = 0.2f;
    //public float maxDropDistance = 0.3f;

    // Start is called before the first frame update
    void Start()
    {

        canvasGroup = GetComponent<CanvasGroup>();
        inventoryController = InventoryController.Instance;

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        //transform.SetParent(transform.root);
        transform.SetParent(GetComponentInParent<Canvas>().transform, true);

        transform.localScale = Vector3.one; // preserve scale

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot originalSlot = originalParent.GetComponent<Slot>();
        if (originalSlot == null)
        {
            Debug.LogError("Original slot is null in OnEndDrag.");
            transform.SetParent(originalParent);
            return;
        }

        Item draggedItem = GetComponent<Item>();
        if (draggedItem == null)
        {
            Debug.LogError("Dragged object has no Item component.");
            transform.SetParent(originalParent);
            return;
        }

        bool overInventory = IsWithinInventory(eventData.position);
        bool overHotbar = IsOverHotbar(eventData.position);

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();
        if (dropSlot == null && eventData.pointerEnter != null)
            dropSlot = eventData.pointerEnter.GetComponentInParent<Slot>();

        // Normal inventory handling

        if (dropSlot != null)
        {
            Item slotItem = dropSlot.currentItem?.GetComponent<Item>();

            if (slotItem != null && draggedItem.CanStackWith(slotItem))
            {
                slotItem.AddToStack(draggedItem.quantity);
                if (originalSlot.currentItem == gameObject)
                    originalSlot.currentItem = null;
                Destroy(gameObject);
                return;
            }

            if (dropSlot.currentItem != null)
            {
                GameObject other = dropSlot.currentItem;
                other.transform.SetParent(originalSlot.transform);
                other.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                originalSlot.currentItem = other;

                transform.SetParent(dropSlot.transform);
                SnapToSlot(transform.GetComponent<RectTransform>());
                dropSlot.currentItem = gameObject;
                return;
            }

            if (originalSlot.currentItem == gameObject)
                originalSlot.currentItem = null;

            transform.SetParent(dropSlot.transform);
            SnapToSlot(transform.GetComponent<RectTransform>());
            dropSlot.currentItem = gameObject;
            return;
            

        }
        else
        {
            // Outside any UI -> drop into world
            if(!overInventory && !overHotbar)
            {
                DropStackedItem(originalSlot);
                return;
            }
            else
            {
                transform.SetParent(originalSlot.transform);
                SnapToSlot(transform.GetComponent<RectTransform>());
                originalSlot.currentItem = gameObject;
                return;
            }
            
        }

    }




    // helper to snap UI item into center of the slot reliably
    private void SnapToSlot(RectTransform rect)
    {
        if (rect == null) return;
        rect.localScale = Vector3.one;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
    }

    // Helper to detect hotbar
    bool IsOverHotbar(Vector2 mousePosition)
    {
        RectTransform hotbarRect = FindObjectOfType<HotbarController>().hotbarPanel.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(hotbarRect, mousePosition);
    }

    bool IsWithinInventory(Vector2 mousePosition)
    {
        RectTransform inventoryRect = originalParent.parent.GetComponent<RectTransform>();/*originalParent.parent.GetComponent<RectTransform>();*/
        return RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, mousePosition);
    }


    void DropItem(Slot originalSlot)
    {

        originalSlot.currentItem = null;
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("Missing 'Player' tag on Player object.");
            return;
        }

        // Directions: up, down, left, right
        Vector2[] directions = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        float dropDistance = 0.4f;        // very close to player
        float checkRadius = 0.2f;         // small radius to check for obstacles
        LayerMask obstacleMask = LayerMask.GetMask("Default", "Tilemap"); // obstacles only

        // Shuffle directions for randomness
        for (int i = 0; i < directions.Length; i++)
        {
            int randIndex = Random.Range(i, directions.Length);
            (directions[i], directions[randIndex]) = (directions[randIndex], directions[i]);
        }

        Vector2 validDropPosition = Vector2.zero;
        bool found = false;

        foreach (Vector2 dir in directions)
        {
            Vector2 testPos = (Vector2)playerTransform.position + dir * dropDistance;

            // Check for obstacles
            Collider2D obstacle = Physics2D.OverlapCircle(testPos, checkRadius, obstacleMask);
            if (obstacle != null)
                continue;

            // Check for items using tag
            Collider2D[] hits = Physics2D.OverlapCircleAll(testPos, checkRadius);
            bool itemFound = false;
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Item"))
                {
                    itemFound = true;
                    break;
                }
            }

            if (!itemFound)
            {
                validDropPosition = testPos;
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.Log("No valid nearby space to drop item — all directions blocked.");
            // Optionally: return item to inventory
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;
            return;
        }

        // Slight random offset to avoid exact overlap if necessary
        validDropPosition += Random.insideUnitCircle * 0.05f;

        // Drop the item and tag it correctly

        GameObject droppedInstance = Instantiate(gameObject, validDropPosition, Quaternion.identity);
        droppedInstance.transform.localScale = Vector3.one;
        droppedInstance.tag = "Item";
        //
        Item droppedItem = droppedInstance.GetComponent<Item>();
        droppedItem.quantity = 1;

        droppedInstance.GetComponent<BounceEffect>().StartBounce();

        Destroy(gameObject);

        InventoryController.Instance.RebuildItemCounts();

    }


    void DropStackedItem(Slot originalSlot)
    {
        Item item = GetComponent<Item>();
        originalSlot.currentItem = null;
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerTransform == null)
        {
            Debug.LogError("Missing 'Player' tag on Player object.");
            return;
        }
        Vector2[] directions = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        float dropDistance = 0.4f;  // distance from player
        float checkRadius = 0.2f;   // radius to check obstacles/items
        LayerMask obstacleMask = LayerMask.GetMask("Default", "Tilemap");

        for (int i = 0; i < item.quantity; i++)
        {
            // Shuffle directions for each item for randomness
            for (int d = 0; d < directions.Length; d++)
            {
                int randIndex = Random.Range(d, directions.Length);
                (directions[d], directions[randIndex]) = (directions[randIndex], directions[d]);
            }

            Vector2 validDropPos = Vector2.zero;
            bool foundSpot = false;

            foreach (Vector2 dir in directions)
            {
                Vector2 testPos = (Vector2)playerTransform.position + dir * dropDistance;

                // Check for obstacles
                if (Physics2D.OverlapCircle(testPos, checkRadius, obstacleMask) != null) continue;

                // Check for existing items
                Collider2D[] hits = Physics2D.OverlapCircleAll(testPos, checkRadius);
                bool itemFound = false;
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Item"))
                    {
                        itemFound = true;
                        break;
                    }
                }

                if (!itemFound)
                {
                    validDropPos = testPos + Random.insideUnitCircle * 0.05f; // small random offset
                    foundSpot = true;
                    break;
                }
            }

            if (!foundSpot)
            {
                // If no valid position found, fallback a little further from player
                validDropPos = (Vector2)playerTransform.position + Random.insideUnitCircle * (dropDistance + 0.3f);
            }

            // Instantiate a single item at the found position
            GameObject dropped = Instantiate(gameObject, validDropPos, Quaternion.identity);
            dropped.transform.localScale = Vector3.one;
            dropped.tag = "Item";
            dropped.GetComponent<Item>().quantity = 1;
            dropped.GetComponent<BounceEffect>().StartBounce();
        }

        Destroy(gameObject);

        InventoryController.Instance.RebuildItemCounts();
    }

    

}

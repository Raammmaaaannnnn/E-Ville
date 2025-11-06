using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;

    //public float minDropDistance = 0.2f;
    //public float maxDropDistance = 0.3f;

    // Start is called before the first frame update
    void Start()
    {

        canvasGroup = GetComponent<CanvasGroup>();

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root);

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

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();

        if (dropSlot == null)
        {
            GameObject dropItem = eventData.pointerEnter;
            if (dropItem != null)
            {
                dropSlot = dropItem.GetComponentInParent<Slot>();
            }
        }
        Slot originalSlot = originalParent.GetComponent<Slot>();
        if (dropSlot != null)
        {
            if (dropSlot.currentItem != null)
            {
                dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                originalSlot.currentItem = dropSlot.currentItem;
                dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            }
            else
            {
                originalSlot.currentItem = null;
            }

            transform.SetParent(dropSlot.transform);

            transform.localScale = Vector3.one;

            dropSlot.currentItem = gameObject;
        }
        else
        {
            if(!IsWithinInventory(eventData.position))
            {
                DropItem(originalSlot);
            }
            else
            {
                transform.SetParent(originalParent);

                transform.localScale = Vector3.one;
            }
        }

        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    bool IsWithinInventory(Vector2 mousePosition)
    {
        RectTransform inventoryRect = originalParent.parent.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, mousePosition);
    }

    void DropItem(Slot originalSlot)
    {
        //originalSlot.currentItem = null;

        //Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        //if(playerTransform == null)
        //{
        //    Debug.LogError("Missing 'Player' tag");
        //    return;
        //}

        //Vector2 dropOffset = Random.insideUnitCircle.normalized * Random.Range(minDropDistance, maxDropDistance);
        //Vector2 dropPosition = (Vector2)playerTransform.position + dropOffset;

        //Instantiate(gameObject, dropPosition, Quaternion.identity);
        //transform.localScale = Vector3.one;
        //Destroy(gameObject);
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

        Destroy(gameObject);
    }

   
}

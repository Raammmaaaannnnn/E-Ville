using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    public bool isOpened { get; private set; }
    public string chestID { get; private set; }

    public GameObject itemPrefab;
    public Sprite openedSprite;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        chestID ??= GlobalHelper.GenerateUniqueID(gameObject);
        animator = GetComponent<Animator>();
    }

    public bool canInteract()
    {
        return !isOpened;
    }

    public void Interact()
    {
        if(!canInteract()) return;

        OpenChest();
    }

    private void OpenChest()
    {
        
        SetOpened(true);
        animator.SetTrigger("isOpened?");
        //dropitem
        if(itemPrefab)
        {
            GameObject droppedItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
            //droppedItem.GetComponent<BounceEffect>()
        }
    }

    public void SetOpened(bool opened)
    {
        
        if(isOpened = opened)
        {
            GetComponent<SpriteRenderer>().sprite = openedSprite;
        }
    }
}

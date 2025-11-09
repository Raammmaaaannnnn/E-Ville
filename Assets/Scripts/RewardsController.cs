using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RewardsController : MonoBehaviour
{
    public static RewardsController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void GiveQuestReward(Quest quest)
    {
        if (quest?.questRewards == null) return;

        foreach (var reward in quest.questRewards)
        {
            switch (reward.type)
            {

                case RewardType.Alcohol:
                    //Give alcohol only by bartender
                    GiveItemReward(reward.rewardID, reward.amount);
                    break;
                case RewardType.Money:
                    // Coins are just added to UI / player wallet
                    GiveCoinReward(reward.rewardID, reward.amount);
                    break;
                case RewardType.Custom:
                    //custom case for future scenarios
                    break;
            }
        }
    }


    public void GiveItemReward(int itemID, int amount)
    {
        var itemPrefab = FindAnyObjectByType<ItemDictionary>()?.GetItemPrefab(itemID);

        if (itemPrefab == null) return;

        for (int i = 0; i < amount; i++)
        {
            if (!InventoryController.Instance.AddItem(itemPrefab))
            {

                GameObject dropItem = Instantiate(itemPrefab, transform.position + (Vector3.down * 1f), Quaternion.identity);
                dropItem.GetComponent<BounceEffect>().StartBounce();
            }
            else
            {
                itemPrefab.GetComponent<Item>().ShowPopup();
            }

        }
    }

    public void GiveCoinReward(int itemID, int amount)
    {
        var itemPrefab = FindAnyObjectByType<ItemDictionary>()?.GetItemPrefab(itemID);

        if (itemPrefab == null) return;

        // Update the coin UI
        CoinUIController.Instance?.AddCoins(amount);

        // Find player or popup anchor
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Spawn temporary popup near player
        Vector3 popupPos = playerTransform != null ? playerTransform.position + Vector3.up * 1.5f : transform.position;
        GameObject tempPopup = Instantiate(itemPrefab, popupPos, Quaternion.identity);

        var item = tempPopup.GetComponent<Item>();
        if (item != null)
        {
            item.ShowPopup();
            SoundEffectManager.Play("Earned");
        }

        Destroy(tempPopup, 1.5f);


    }

}

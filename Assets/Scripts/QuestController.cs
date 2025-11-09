using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Pathfinding.RaycastModifier;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance {  get; private set; }
    public List<QuestProgress> activateQuests = new();
    private QuestUI questUI;

    public List<string> handinQuestIDs = new();
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        questUI = FindObjectOfType<QuestUI>();
        InventoryController.Instance.OnInventoryChanged += CheckInventoryForQuests;

    }

    public void  AcceptQuest(Quest quest)
    {
        if (IsQuestActive(quest.questID)) return;

        activateQuests.Add(new QuestProgress(quest));
        CheckInventoryForQuests();
        questUI.UpdateQuestUI();
    }

    public bool IsQuestActive(string questID) => activateQuests.Exists(q => q.QuestID == questID);



    public void CheckInventoryForQuests()
    {
        Dictionary<(int id, int variantID), int> itemCounts = InventoryController.Instance.GetItemCounts();

        foreach(QuestProgress quest in activateQuests)
        {
            foreach(QuestObjective questObjective in quest.objectives)
            {
                if(questObjective.type != ObjectiveType.CollectItem) continue;
                if(!int.TryParse(questObjective.objectiveID, out int itemID)) continue;

                // Use variantID from questObjective if defined, else 0
                int variantID = questObjective.variantID; // Add variantID field to QuestObjective

                var key = (itemID, variantID);


                int newAmount = itemCounts.TryGetValue(key, out int count) ? Mathf.Min(count, questObjective.requiredAmount) : 0;

                if(questObjective.currentAmount != newAmount)
                {
                    questObjective.currentAmount = newAmount;
                }
            }
        }

        questUI.UpdateQuestUI();
    }

    public bool IsQuestCompleted(string questID)
    {
        QuestProgress quest = activateQuests.Find(q => q.QuestID == questID);
        return quest != null && quest.objectives.TrueForAll(o => o.IsCompleted);

    }

    public void HandInQuest(string questId)
    {
        //try rome required items
        if(!RemoveRequiredItemsFromInventroy(questId))
        {
            return;
        }

        //remove quest from quest log
        QuestProgress quest = activateQuests.Find(q => q.QuestID == questId);
        if(quest != null)
        {
            handinQuestIDs.Add(questId);
            activateQuests.Remove(quest);
            questUI.UpdateQuestUI();
        }
    }

    public bool IsQuestHandedIn(string questId)
    {
        return handinQuestIDs.Contains(questId);
    }


    public bool RemoveRequiredItemsFromInventroy(string questID)
    {
        QuestProgress quest = activateQuests.Find(q => q.QuestID == questID);
        if(quest == null) return false;

        Dictionary<(int id, int variantID), int> requiredItems = new();

        //Item requirement from objectives
        foreach(QuestObjective objective in quest.objectives) 
        {
            if(objective.type == ObjectiveType.CollectItem && int.TryParse(objective.objectiveID, out int itemID))
            {
                int variantID = objective.variantID;
                var Key = (itemID, variantID);
                requiredItems[Key] = objective.requiredAmount; 
            }

        }

        //Verify we have items
        Dictionary<(int id, int variantID), int> itemCounts = InventoryController.Instance.GetItemCounts();
        foreach(var item in requiredItems)
        {
            if(itemCounts.GetValueOrDefault(item.Key) < item.Value)
            {
                return false; // not enough items
            }
        }

        //Remove required items from inventory
        foreach(var itemRequirement in requiredItems)
        {
            //remove items from inventory
            int itemID = itemRequirement.Key.id;
            int variantID = itemRequirement.Key.variantID;
            int amountToRemove = itemRequirement.Value;

            InventoryController.Instance.RemoveItemsFromInventory(itemID, variantID, amountToRemove);

        }

        return true;
    }


    public void LoadQuestProgress(List<QuestProgress> savedQuests)
    {
        activateQuests = savedQuests ?? new();

        CheckInventoryForQuests();
        questUI.UpdateQuestUI();
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public static QuestUI Instance { get; private set; }
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject objectiveTextPrefab;

    public Quest testQuest;
    public int testQuestAmount;
    //private List<QuestProgress> testQuests = new(); //for testing quests

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        //for(int i = 0; i < testQuestAmount; i++)
        //{
        //    test.Add(new QuestProgress(testQuest)); //to test replace and fill the inspector
        //}

        UpdateQuestUI();
    }


    public void UpdateQuestUI()
    {
        foreach(Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        //build quest entries
        foreach(var quest in QuestController.Instance.activateQuests) //replace her after in ... to test
        {
            GameObject entry = Instantiate(questEntryPrefab, questListContent);
            TMP_Text questNameText = entry.transform.Find("QuestNameText").GetComponent<TMP_Text>();
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            questNameText.text = quest.quest.name;

            foreach(var objective in quest.objectives)
            {
                GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();
                objText.text = $"{objective.description}({objective.currentAmount}/{objective.requiredAmount})"; // Collect 5 Jars ( 0/5)
            }
        }
    }
}

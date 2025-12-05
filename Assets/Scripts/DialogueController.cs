using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image potraitImage;
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;
    public NPC currentNPC;
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
            DDOLTracker.Register(gameObject); // <- Track this DDOL object
        }
        else
        {
            Destroy(gameObject);
        }
    
    }
    
    public void ShowDialogueUI(bool show)
    {
        dialoguePanel.SetActive(show);
    }

    public void SetNPCInfo(string npcName, Sprite portrait)
    {
        nameText.text = npcName;
        potraitImage.sprite = portrait;
    }

    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
    }

    public void ClearChoices()
    {
        foreach(Transform child in choiceContainer) Destroy(child.gameObject);
    }

    public void Onpress()
    {
        // Find the NPC in the scene currently tagged "NPC" and with active dialogue
        NPC currentNPC = null;
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
        foreach (GameObject go in npcs)
        {
            NPC candidate = go.GetComponent<NPC>();
            if (candidate != null && candidate.isDialogueActive)
            {
                currentNPC = candidate;
                break;
            }
        }

        if (currentNPC == null) return;

        currentNPC.isTyping = false;
        currentNPC.EndDialogue();
    }

    public GameObject CreateChoiceButton(string choiceText, UnityEngine.Events.UnityAction onClick)
    {
        GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
        choiceButton.GetComponentInChildren<TMP_Text>().text = choiceText;
        choiceButton.GetComponent<Button>().onClick.AddListener(onClick);
        return choiceButton;
    }
}

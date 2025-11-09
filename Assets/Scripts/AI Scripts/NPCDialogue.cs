using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]

public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPotrait;
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public bool[] endDialougeLines; //Mark end of dialogue
    public float autoProgressDelay = 0.9f;
    public float typingSpeed = 0.05f;
    public AudioClip voiceSound;
    public float voicePitch = 1f;

    public DialogueChoice[] choices;

    public int questInProgressIndex;// when we in quest what will npc say
    public int questCompleteIndex; // when we complete what will npc say
    public Quest quest; //npc gives this quest
    
}

[System.Serializable]

public class DialogueChoice
{
    public int dialogueIndex;//Dialogue UI
    public string[] choices; //Player response options
    public int[] nextDialogueIndexes; //Where choice leads
    public bool[] givesQuest; // which choice gives quest
    public bool[] spendsCoins; // new field, same size as choices
    public int[] coinCost;     // how many coins each choice costs
    public int[] alcoholRewardID; // ID of alcohol item given after spending coins

}

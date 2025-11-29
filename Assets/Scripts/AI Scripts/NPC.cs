using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;

    private DialogueController dialogueUI;  
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    public bool isAttackable = false; // runtime flag

    [Header("Patrol Settings")]
    public Transform patrolParent;
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    public bool loopPatrolPoints = true;
    private bool canPatrol = true;

    [Header("Movement Settings")]
    public float speed = 0.4f;

    public SpriteRenderer spriteRenderer;
    public Animator animator;

    [Header("Flee Settings")]
    public float fleeSpeed = 0.6f;
    public float fleeDistance = 5f;
    private bool isFleeing = false;
    private Vector3 fleeDirection;

    private enum QuestState { NotStarted, InProgress, Completed}
    private QuestState questState = QuestState.NotStarted;

    private void Awake()
    {
        // Optional: cache the instance once
        if (DialogueController.Instance != null)
        {
            dialogueUI = DialogueController.Instance;
        }


    }

    private void Start()
    {
        // Initialize patrol points from parent if assigned
        if (patrolParent != null)
        {
            patrolPoints = new Transform[patrolParent.childCount];
            for (int i = 0; i < patrolParent.childCount; i++)
            {
                patrolPoints[i] = patrolParent.GetChild(i);
            }
        }
    }

    public void UpdateAttackableStatus()
    {
        PlayerIntoxication playerIntox = FindObjectOfType<PlayerIntoxication>();
        if (playerIntox != null)
        {
            isAttackable = playerIntox.currentLevel >= PlayerIntoxication.IntoxicationLevel.Orange;
        }
    }

    private void Update()
    {
        UpdateAttackableStatus(); // constantly check player's state
        Debug.Log("NPC Update → canPatrol: " + canPatrol  + " | isDialogueActive: " + isDialogueActive);
        // Stop patrol during dialogue or waiting
        if (isDialogueActive || !canPatrol)
        {
            animator.SetBool("NPCMoving", false);
            return;
        }

        if (isFleeing)
        {
            FleeFromPlayer();
            return;
        }


        // Patrol at all times when free
        Patrol();
        
    }

    //

    public void TakeDamage(int damage, Vector3 playerPosition)
    {
        if (!isAttackable) return;

        // Trigger fleeing
        isFleeing = true;
        fleeDirection = (transform.position - playerPosition).normalized;

        // Stop patrol
        canPatrol = false;
        animator.SetBool("NPCMoving", true);

        Debug.Log($"{name} is fleeing!");
    }

    void FleeFromPlayer()
    {
        if (!isFleeing) return;

        Vector3 newPos = transform.position + fleeDirection * fleeSpeed * Time.deltaTime;

        // Check collisions with obstacles
        RaycastHit2D hit = Physics2D.Raycast(transform.position, fleeDirection, fleeSpeed * Time.deltaTime);
        if (hit.collider != null && hit.collider.gameObject.CompareTag("Obstacle"))
        {
            // Reflect direction randomly to avoid obstacle
            fleeDirection = Vector3.Reflect(fleeDirection, hit.normal).normalized;
        }

        transform.position += fleeDirection * fleeSpeed * Time.deltaTime;

        // Flip sprite
        spriteRenderer.flipX = fleeDirection.x < 0;

        // Optional: stop fleeing after distance
        if (Vector3.Distance(transform.position, fleeDirection + transform.position) > fleeDistance)
        {
            isFleeing = false;
            canPatrol = true;
        }
    }

    ////
    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];

        // Move NPC toward target using Vector3
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            speed * Time.deltaTime
        );

        // Flip sprite based on direction
        float dirX = targetPoint.position.x - transform.position.x;
        if (dirX > 0.01f) spriteRenderer.flipX = false;
        else if (dirX < -0.01f) spriteRenderer.flipX = true;

        animator.SetBool("NPCMoving", true);

        // Check if reached patrol point
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.13f)
        {
            currentPatrolIndex = loopPatrolPoints
                ? (currentPatrolIndex + 1) % patrolPoints.Length
                : Mathf.Min(currentPatrolIndex + 1, patrolPoints.Length - 1);
        }
    }


    public bool canInteract()
    {
        return !isDialogueActive; 
    }

    public void Interact()
    {
        

        if (dialogueData == null || (PauseController.IsGamePaused && !isDialogueActive))
        {
            return;
        }
        
        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }
    
    void StartDialogue()
    {
        // Ensure dialogueUI is assigned
        if (dialogueUI == null)
        {
            if (DialogueController.Instance == null)
            {
                Debug.LogWarning("⚠️ DialogueController.Instance is null — no UI found!");
                return;
            }
            dialogueUI = DialogueController.Instance;
        }

        //sync with quest
        SyncQuestState();

        //set dialogue line based on quest state
        if(questState == QuestState.NotStarted)
        {
            dialogueIndex = 0;
        }
        else if (questState == QuestState.InProgress)
        {
            dialogueIndex = dialogueData.questInProgressIndex;
        }
        else if(questState == QuestState.Completed)
        {
            dialogueIndex = dialogueData.questCompleteIndex;
        }
        else
        {
            dialogueIndex = 0;
        }

        canPatrol = false;  // Stop patrolling
        animator.SetBool("NPCMoving", false);


        isDialogueActive = true;
        

        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPotrait);
        dialogueUI.ShowDialogueUI(true);
        
        PauseController.SetPause(true);
        DisplayCurrentLine();
    }

    private void SyncQuestState()
    {
        if (dialogueData.quest == null) return;

        string questID = dialogueData.quest.questID;

        //update add completing quest and handing in reward
        if(QuestController.Instance.IsQuestCompleted(questID) || QuestController.Instance.IsQuestHandedIn(questID))
        {
            questState = QuestState.Completed;
        }
        else if(QuestController.Instance.IsQuestActive(questID))
        {
            questState = QuestState.InProgress;

        }
        else
        {
            questState = QuestState.NotStarted;
        }


    }

    void NextLine()
    {
        if(isTyping)
        {
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }

        dialogueUI.ClearChoices();

        if(dialogueData.endDialougeLines.Length > dialogueIndex && dialogueData.endDialougeLines[dialogueIndex] )
        {
            EndDialogue();
            return;
        }

        foreach(DialogueChoice dialogueChoice in dialogueData.choices)
        {
            if(dialogueChoice.dialogueIndex == dialogueIndex)
            {
                DisplayChoices(dialogueChoice);
                return;
            }
        }


        if(++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueUI.SetDialogueText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueUI.SetDialogueText(
                dialogueUI.dialogueText.text += letter
            );
            SoundEffectManager.PlayVoice(dialogueData.voiceSound, dialogueData.voicePitch);
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping=false;

        if(dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    void DisplayChoices(DialogueChoice choice)
    {
        for(int i = 0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndexes[i];
            bool givesQuest = choice.givesQuest[i];

            // New: coin spending
            bool spendsCoins = choice.spendsCoins != null && choice.spendsCoins.Length > i && choice.spendsCoins[i];
            int coinCost = choice.coinCost != null && choice.coinCost.Length > i ? choice.coinCost[i] : 0;

            int rewardID = choice.RewardID != null && choice.RewardID.Length > i ? choice.RewardID[i] : 0;

            //dialogueUI.CreateChoiceButton(choice.choices[i], () => ChooseOption(nextIndex, givesQuest) );

            dialogueUI.CreateChoiceButton(
                choice.choices[i],
                () => ChooseOption(nextIndex, givesQuest, spendsCoins, coinCost, rewardID));
        }
    }

    void ChooseOption(int nextIndex, bool givesQuest /*here*/ , bool spendsCoins = false, int coinCost = 0, int RewardID = 0)
    {

        if (spendsCoins)
        {
            if (CoinUIController.Instance.GetCurrentCoins() >= coinCost)
            {
                CoinUIController.Instance.SpendCoins(coinCost);
                // optional: trigger item/bonus after spending
                Debug.Log($"Spent {coinCost} coins for choice {nextIndex}");
                
            }
            else
            {
                Debug.Log("Not enough coins!");
                return; // prevent progressing to next line if insufficient coins
            }
           
        }
        // Give reward
        if (RewardID != 0)
        {
            Debug.Log($"Giving reward item: ID={RewardID}");
            RewardsController.Instance.GiveItemReward(RewardID, 1);
        }

        if (givesQuest)
        {
            QuestController.Instance.AcceptQuest(dialogueData.quest);
            questState = QuestState.InProgress;
        }

        dialogueIndex = nextIndex;
        dialogueUI.ClearChoices();
        DisplayCurrentLine();

    }

    void DisplayCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    public void EndDialogue()
    {
        if (questState == QuestState.Completed && !QuestController.Instance.IsQuestHandedIn(dialogueData.quest.questID))
        {
            //handle quest completion
            HandleQuestCompletion(dialogueData.quest);

        }

        StopAllCoroutines();
        isDialogueActive = false;
        dialogueUI.SetDialogueText("");
        dialogueUI.ShowDialogueUI(false);
        PauseController.SetPause(false);


        canPatrol = true; // Resume patrol after talking
        

    }


    void HandleQuestCompletion(Quest quest)
    {
        RewardsController.Instance.GiveQuestReward(quest);
        QuestController.Instance.HandInQuest(quest.questID);
    }

    
}

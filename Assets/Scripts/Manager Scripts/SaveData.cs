using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class SaveData
{
    public string sceneName; // New: stores which scene to load
    public Vector3 playerPosition;
    public List<InventorySaveData> inventorySaveData;
    public List<InventorySaveData> hotbarSaveData;
    public List<ChestSaveData> chestSaveData;
    public List<QuestProgress> questProgressData;
    public List<string> handinQuestIds;
    public int coins; 
}


[System.Serializable]

public class ChestSaveData
{
    public string chestID;
    public bool isOpened;
}
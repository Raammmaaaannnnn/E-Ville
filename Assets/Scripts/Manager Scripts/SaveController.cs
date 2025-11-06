using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement; // Needed for scene info

public class SaveController : MonoBehaviour
{

    private string saveLocation;
    private InventoryController inventoryController;
    private HotbarController hotbarController;
    private Chest[] chests;


    // Start is called before the first frame update
    void Start()
    {
        InitializeComponents();
        LoadGame();
    }

    private void InitializeComponents()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindObjectOfType<InventoryController>();
        hotbarController = FindObjectOfType<HotbarController>();
        chests = FindObjectsOfType<Chest>();
    }

   public void SaveGame()
    {
        SaveData saveData = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name, // Save current scene
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            inventorySaveData = inventoryController.GetInventoryItems(),
            hotbarSaveData = hotbarController.GetHotbarItems(),
            chestSaveData = GetChestsState(),
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
        

    }

    private List<ChestSaveData> GetChestsState()
    {
        List<ChestSaveData> chestStates = new List<ChestSaveData>();

        foreach (Chest chest in chests)
        {
            ChestSaveData chestSaveData = new ChestSaveData
            {
                chestID = chest.chestID,
                isOpened = chest.isOpened,
            };

            chestStates.Add(chestSaveData);
        }

        return chestStates;
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {

            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));


            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition;

            inventoryController.SetInventoryItems(saveData.inventorySaveData);
            hotbarController.SetHotbarItems(saveData.hotbarSaveData);

            LoadChestStates(saveData.chestSaveData);
        }
        else
        {
            SaveGame();

            inventoryController.SetInventoryItems(new List<InventorySaveData>());
            hotbarController.SetHotbarItems(new List<InventorySaveData>());
        }
    }

    private void LoadChestStates(List<ChestSaveData> chestStates)
    {
        foreach(Chest chest in chests)
        {
            ChestSaveData chestSaveData = chestStates.FirstOrDefault(c => c.chestID == chest.chestID);

            if(chestSaveData != null)
            {
                chest.SetOpened(chestSaveData.isOpened);
            }

        }
    }

}

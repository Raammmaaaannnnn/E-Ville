
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement; // Needed for scene info

public class SaveController : MonoBehaviour
{
    public static SaveController Instance;
    private string saveLocation;
    private InventoryController inventoryController;
    private HotbarController hotbarController;
    private Chest[] chests;
    private Destructible[] destructibles;
    public Transform startgame;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else
        {
            Destroy(gameObject);
        }
    }

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
        destructibles = FindObjectsOfType<Destructible>();
    }

    public void SaveGame()
    {
        InitializeComponents();
        SaveData saveData = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name, // Save current scene
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            inventorySaveData = inventoryController.GetInventoryItems(),
            hotbarSaveData = hotbarController.GetHotbarItems(),
            chestSaveData = GetChestsState(),
            DesSaveData = GetDesState(),
            questProgressData = QuestController.Instance.activateQuests,
            handinQuestIds = QuestController.Instance.handinQuestIDs,
            coins = CoinUIController.Instance.GetCurrentCoins() // SAVE COINS
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

    private List<DesSaveData> GetDesState()
    {
        List<DesSaveData> DesStates = new List<DesSaveData>();

        foreach (Destructible destructible in destructibles)
        {
            DesSaveData desSaveData = new DesSaveData
            {
                DesID = destructible.DesID,
                destroyed = destructible.destroyed,
            };

            DesStates.Add(desSaveData);
        }

        return DesStates;
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

            QuestController.Instance.LoadQuestProgress(saveData.questProgressData);


            QuestController.Instance.handinQuestIDs = saveData.handinQuestIds;

            // LOAD COINS
            if (CoinUIController.Instance != null)
                CoinUIController.Instance.SetCoins(saveData.coins);
        }
        else
        {

            Debug.Log("No save found. Starting a new game...");

            StartNewGame();
            //SaveGame();
            //GameObject.FindGameObjectWithTag("Player").transform.position = startgame.transform.position;
            //inventoryController.SetInventoryItems(new List<InventorySaveData>());
            //hotbarController.SetHotbarItems(new List<InventorySaveData>());
        }
    }

    // ------------------ NEW GAME -----------------------
    public void StartNewGame()
    {
        InitializeComponents();

        GameObject.FindGameObjectWithTag("Player").transform.position = startgame.position;

        inventoryController.SetInventoryItems(new List<InventorySaveData>());
        hotbarController.SetHotbarItems(new List<InventorySaveData>());

        foreach (Chest c in chests)
            c.SetOpened(false);

        foreach (Destructible d in destructibles)
            d.SetDestroyed(false);
    }

    // ------------------ DELETE SAVE -----------------------
    public static void DeleteSave()
    {
        string path = Path.Combine(Application.persistentDataPath, "saveData.json");
        Debug.Log("Attempting to delete: " + path);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Save file DELETED at: " + path);
        }
        else
        {
            Debug.Log("No save file found.");
        }

    }


    private void LoadChestStates(List<ChestSaveData> chestStates)
    {
        foreach (Chest chest in chests)
        {
            ChestSaveData chestSaveData = chestStates.FirstOrDefault(c => c.chestID == chest.chestID);

            if (chestSaveData != null)
            {
                chest.SetOpened(chestSaveData.isOpened);
            }

        }
    }

    private void LoadDesStates(List<DesSaveData> desStates)
    {

        foreach (Destructible destructible in destructibles)
        {
            DesSaveData desSaveData = desStates.FirstOrDefault(d => d.DesID == destructible.DesID);

            if (desSaveData != null)
            {
                destructible.SetDestroyed(desSaveData.destroyed);
            }

            
        }

    }

  

}

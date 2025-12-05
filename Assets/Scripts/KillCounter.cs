using UnityEngine;
using TMPro;

public class KillCounter : MonoBehaviour
{
    public static KillCounter instance;

    [Header("UI")]
    public TMP_Text killText;

    private int kills = 0;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void AddKill(int amount = 1)
    {
        kills += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (killText != null)
            killText.text = $"Kills: {kills}";
    }

    public void ResetKills()
    {
        kills = 0;
        UpdateUI();
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinUIController : MonoBehaviour
{
    public static CoinUIController Instance { get; private set; }

    [SerializeField] private TMP_Text coinText;
    [SerializeField] private Image coinIcon;

    private int currentCoins = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        UpdateCoinUI();
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        UpdateCoinUI();
        Debug.Log($"+{amount} coins!");
    }

    public void SpendCoins(int amount)
    {
        currentCoins = Mathf.Max(0, currentCoins - amount);
        UpdateCoinUI();
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = currentCoins.ToString();
        
    }


    public int GetCurrentCoins()
    {
        return currentCoins;
    }

    public void SetCoins(int amount)
    {
        currentCoins = Mathf.Max(0, amount);
        UpdateCoinUI();
    }

    public int GetCoinCount() => currentCoins;
}

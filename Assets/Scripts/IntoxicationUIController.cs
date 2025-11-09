using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntoxicationUIController : MonoBehaviour
{
    public static IntoxicationUIController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image fillBar;
    [SerializeField] private TMP_Text levelText;

    [Header("Colors")]
    public Color noneColor = Color.gray;
    public Color greenColor = Color.green;
    public Color orangeColor = new Color(1f, 0.5f, 0f); // orange
    public Color redColor = Color.red;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void UpdateIntoxicationUI(PlayerIntoxication.IntoxicationLevel level, float progress)
    {
        if (fillBar == null || levelText == null) return;

        // Smooth bar fill (0–1)
        fillBar.fillAmount = Mathf.Clamp01(progress);

        // Update color and label
        switch (level)
        {
            case PlayerIntoxication.IntoxicationLevel.Green:
                fillBar.color = greenColor;
                levelText.text = "Ecstatic";
                break;
            case PlayerIntoxication.IntoxicationLevel.Orange:
                fillBar.color = orangeColor;
                levelText.text = "Tipsy";
                break;
            case PlayerIntoxication.IntoxicationLevel.Red:
                fillBar.color = redColor;
                levelText.text = "Wasted";
                break;
            default:
                fillBar.color = noneColor;
                levelText.text = "Sober";
                break;
        }
    }
}

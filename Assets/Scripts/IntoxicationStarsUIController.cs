using UnityEngine;
using UnityEngine.UI;

public class IntoxicationStarsUIController : MonoBehaviour
{
    public static IntoxicationStarsUIController Instance { get; private set; }

    [Header("Star Images")]
    public Image star1;
    public Image star2;
    public Image star3;

    [Header("Sprites")]
    public Sprite emptyStar;
    public Sprite filledStar;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ResetStars();
    }

    /// <summary>
    /// Updates the star UI based on the number of stars earned
    /// </summary>
    /// <param name="stars">0–3 stars</param>
    public void UpdateStars(int stars)
    {
        // Clamp stars to 0–3
        stars = Mathf.Clamp(stars, 0, 3);

        // Set sprites
        star1.sprite = stars >= 1 ? filledStar : emptyStar;
        star2.sprite = stars >= 2 ? filledStar : emptyStar;
        star3.sprite = stars >= 3 ? filledStar : emptyStar;
    }

    /// <summary>
    /// Reset stars to 0
    /// </summary>
    public void ResetStars()
    {
        UpdateStars(0);
    }
}

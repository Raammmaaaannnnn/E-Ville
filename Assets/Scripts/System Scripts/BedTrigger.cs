using UnityEngine;

/// <summary>
/// Attach to the bed area (2D trigger). When the player enters the trigger and the player has 3 stars,
/// show the Sleep UI (SleepUIController). This only handles showing/hiding the UI.
/// Actual sleep/escape logic will be implemented in the Sleep button (step 2).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BedroomTrigger : MonoBehaviour
{
    [Tooltip("Tag used to identify the player GameObject")]
    public string playerTag = "Player";

    private void Reset()
    {
        // ensure the collider is a trigger in the editor
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("TRIGGER HIT: " + other.name);
        if (!other.CompareTag("Player")) return;

        int currentStars = GetCurrentStars();
        if (currentStars >= 3)
        {
            // Show the sleep UI - only available when 3-star is active
            if (SleepUIController.Instance != null)
                SleepUIController.Instance.Show();
            else
                Debug.LogWarning("SleepUIController.Instance is null. Make sure a SleepUIController exists in scene.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
       
        if (!other.CompareTag(playerTag)) return;

        // hide UI if leaving bed area
        if (SleepUIController.Instance != null)
            SleepUIController.Instance.Hide();
    }

    // Defensive helper: try GetStars(), fallback to public field starCount if present
    private int GetCurrentStars()
    {
        if (IntoxicationStarsManager.Instance == null) return 0;
        // Try common API names - use reflection fallback if you changed manager API
#if UNITY_EDITOR
        // Prefer method GetStars() if available
#endif
        // First try method GetStars()
        try
        {
            return IntoxicationStarsManager.Instance.GetStars();
        }
        catch
        {
            // fallback to public field starCount
            try
            {
                // assume starCount exists
                var field = typeof(IntoxicationStarsManager).GetField("starCount");
                if (field != null)
                {
                    object val = field.GetValue(IntoxicationStarsManager.Instance);
                    if (val is int) return (int)val;
                }
            }
            catch { }
        }

        Debug.LogWarning("Unable to read star count from IntoxicationStarsManager. Defaulting to 0.");
        return 0;
    }
}

using UnityEngine;
using System.Collections;

public class DrunkEffect : MonoBehaviour
{
    [Header("Drunk Settings")]
    public float baseDuration = 10f;
    public float staggerStrength = 0.25f;
    public bool isDrunk { get; private set; }
    private float drunkTimer;

    private float drunkStrength; // smooth in/out
    private Camera mainCam;
    private UnityEngine.Rendering.Volume blurVolume;

    void Start()
    {
        mainCam = Camera.main;
        // Optional: find a Post-Processing Volume tagged "BlurEffect"
        blurVolume = FindObjectOfType<UnityEngine.Rendering.Volume>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) && !isDrunk)
            StartCoroutine(BecomeDrunk());
    }

    public IEnumerator BecomeDrunk()
    {
        isDrunk = true;
        float duration = Random.Range(baseDuration * 0.8f, baseDuration * 1.3f);

        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            drunkStrength = Mathf.PingPong(Time.time * 0.5f, 0.7f) + 0.3f;

            if (blurVolume != null)
                blurVolume.weight = drunkStrength;

            yield return null;
        }

        // Sober up
        float fade = 0;
        while (fade < 1f)
        {
            fade += Time.deltaTime;
            drunkStrength = Mathf.Lerp(drunkStrength, 0, fade);

            if (blurVolume != null)
                blurVolume.weight = drunkStrength;

            yield return null;
        }

        isDrunk = false;
    }

    public Vector2 GetDrunkMovementOffset()
    {
        if (!isDrunk) return Vector2.zero;

        float offsetX = (Mathf.PerlinNoise(Time.time * 2f, 0) - 0.5f) * staggerStrength * drunkStrength;
        float offsetY = (Mathf.PerlinNoise(0, Time.time * 2.5f) - 0.5f) * staggerStrength * drunkStrength;
        return new Vector2(offsetX, offsetY);
    }
}

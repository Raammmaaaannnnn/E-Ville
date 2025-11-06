using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player; // assign your player here
    public Vector2 minLimits; // minimum X,Z (or X,Y) for town
    public Vector2 maxLimits; // maximum X,Z (or X,Y) for town

    void LateUpdate()
    {
        Vector3 newPos = player.position;

        // Clamp to town boundaries
        newPos.x = Mathf.Clamp(newPos.x, minLimits.x, maxLimits.x);
        newPos.z = Mathf.Clamp(newPos.z, minLimits.y, maxLimits.y);

        transform.position = newPos;
    }
}

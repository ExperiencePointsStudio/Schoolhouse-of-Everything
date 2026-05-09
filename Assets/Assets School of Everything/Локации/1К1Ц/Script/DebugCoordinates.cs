using UnityEngine;
using TMPro;

public class DebugCoordinates : MonoBehaviour
{
    public Transform player;
    public TMP_Text coordsText;

    void Update()
    {
        if (player != null && coordsText != null)
        {
            Vector3 pos = player.position;
            coordsText.text = $"X: {pos.x:F2}  Y: {pos.y:F2}  Z: {pos.z:F2}";
        }
    }
}

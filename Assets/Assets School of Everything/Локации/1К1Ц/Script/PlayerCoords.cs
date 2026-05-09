using UnityEngine;
using TMPro;

public class PlayerCoords : MonoBehaviour
{
    public Transform player;      // сюда перетащи Player
    public TextMeshProUGUI coordsText; // сюда перетащи CoordsText

    void Update()
    {
        if (player && coordsText)
        {
            Vector3 pos = player.position;
            coordsText.text = $"X:{pos.x:F1}  Y:{pos.y:F1}  Z:{pos.z:F1}";
        }
    }
}

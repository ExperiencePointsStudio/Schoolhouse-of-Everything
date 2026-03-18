using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    public NPCController npc;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        npc.OnPlayerEnter(other.transform);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        npc.OnPlayerExit();
    }
}

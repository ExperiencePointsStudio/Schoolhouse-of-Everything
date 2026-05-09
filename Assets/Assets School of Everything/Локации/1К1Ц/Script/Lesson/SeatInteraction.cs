using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SeatInteraction : MonoBehaviour
{
    [Header("References")]
    public GameObject player;             // Player root (with CharacterController + SOE_PlayerController)
    public GameObject testPaperUI;        // Canvas / UI panel that appears when sitting
    public GameObject tablePaper;         // the paper on the table (to destroy after done) - optional
    public Transform seatPoint;           // transform where player is placed when sits
    public Transform exitPoint;           // transform where player is teleported after finishing (the 'dummy' spawn)
    public GameObject uiHint;             // "Press E to sit" UI element

    [Header("Settings")]
    public KeyCode sitKey = KeyCode.E;

    // state
    private bool isNear = false;
    private bool isSitting = false;
    private bool isUsed = false;

    void Reset()
    {
        // ensure collider is trigger
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void Update()
    {
        if (isNear && !isSitting && !isUsed && Input.GetKeyDown(sitKey))
        {
            SitDown();
        }
    }

    void SitDown()
    {
        if (player == null)
        {
            Debug.LogWarning("SeatInteraction: player not assigned.");
            return;
        }

        if (seatPoint != null)
        {
            player.transform.position = seatPoint.position;
            player.transform.rotation = seatPoint.rotation;
        }

        // disable player movement components safely
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        var pc = player.GetComponent<SOE_PlayerController>();
        if (pc != null) pc.enabled = false;

        if (testPaperUI != null)
            testPaperUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isSitting = true;

        if (uiHint != null)
            uiHint.SetActive(false);
    }

    // вызывается из SimpleTest.CloseTest() или аналогично
    public void FinishLesson()
    {
        // prevent reusing
        isUsed = true;

        if (testPaperUI != null)
            testPaperUI.SetActive(false);

        // re-enable player controller
        if (player != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = true;

            var pc = player.GetComponent<SOE_PlayerController>();
            if (pc != null) pc.enabled = true;

            // teleport to exit point if assigned to avoid getting stuck in seat
            if (exitPoint != null)
            {
                player.transform.position = exitPoint.position;
                player.transform.rotation = exitPoint.rotation;
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isSitting = false;

        if (uiHint != null)
            uiHint.SetActive(false);

        if (tablePaper != null)
            Destroy(tablePaper);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player && !isUsed)
        {
            isNear = true;
            if (uiHint != null) uiHint.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            isNear = false;
            if (uiHint != null) uiHint.SetActive(false);
        }
    }
}

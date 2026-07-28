using System.Collections;
using UnityEngine;

public class NPCShopText : MonoBehaviour
{
    public Transform npc;
    public GameObject textMesh;
    public Vector3 offset = new Vector3(0, 2, 0);
    private Vector3 lastNpcPosition;

    [Header("Time")]
    [SerializeField] 
    public float time1 =1.5f;
    [SerializeField]
    public float time2 = 1.5f;

    void Start()
    {
        UpdatePosition(true);
        StartCoroutine(ShowShopText());
    }

    private void UpdatePosition(bool force)
    {
        if (npc != null && (force || npc.position != lastNpcPosition))
        {
            lastNpcPosition = npc.position;
            transform.position = lastNpcPosition + offset;
        }
    }

    IEnumerator ShowShopText()
    {
        WaitForSeconds visibleDuration = new WaitForSeconds(time1);
        WaitForSeconds hiddenDuration = new WaitForSeconds(time2);
        while (true)
        {
            // NPCs are normally static. Refresh only when the label becomes
            // visible instead of polling the transform every frame.
            UpdatePosition(false);
            textMesh.gameObject.SetActive(true); 
            yield return visibleDuration;
            textMesh.gameObject.SetActive(false); 
            yield return hiddenDuration;
        }
    }
}

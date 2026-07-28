using System.Collections;
using UnityEngine;

public class HatMan : MonoBehaviour
{
    public Transform npc;
    public GameObject textMesh;
    public Vector3 offset = new Vector3(0, 2, 0);
    private Vector3 lastNpcPosition;

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
        WaitForSeconds visibleDuration = new WaitForSeconds(2f);
        WaitForSeconds hiddenDuration = new WaitForSeconds(3f);
        while (true)
        {
            UpdatePosition(false);
            textMesh.gameObject.SetActive(true); 
            yield return visibleDuration;
            textMesh.gameObject.SetActive(false); 
            yield return hiddenDuration;
        }
    }
}

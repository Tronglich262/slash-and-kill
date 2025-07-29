using System.Collections;
using UnityEngine;
using TMPro;

public class NPCShopText : MonoBehaviour
{
    public Transform npc;
    public GameObject textMesh;
    public Vector3 offset = new Vector3(0, 2, 0);

    void Start()
    {
        StartCoroutine(ShowShopText());
    }

    void Update()
    {
        if (npc != null)
        {
            transform.position = npc.position + offset;
        }
    }

    IEnumerator ShowShopText()
    {
        while (true)
        {
            textMesh.gameObject.SetActive(true); // Hiện chữ
            yield return new WaitForSeconds(1.5f);
            textMesh.gameObject.SetActive(false); // Ẩn chữ
            yield return new WaitForSeconds(1.5f);
        }
    }
}
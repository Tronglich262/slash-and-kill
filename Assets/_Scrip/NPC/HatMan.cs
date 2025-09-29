using System.Collections;
using UnityEngine;
using TMPro;

public class HatMan : MonoBehaviour
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
            yield return new WaitForSeconds(2f);
            textMesh.gameObject.SetActive(false); // Ẩn chữ
            yield return new WaitForSeconds(3f);
        }
    }
}
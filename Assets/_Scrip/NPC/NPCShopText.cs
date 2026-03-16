using System.Collections;
using UnityEngine;
using TMPro;

public class NPCShopText : MonoBehaviour
{
    public Transform npc;
    public GameObject textMesh;
    public Vector3 offset = new Vector3(0, 2, 0);

    [Header("Time")]
    [SerializeField] 
    public float time1 =1.5f;
    [SerializeField]
    public float time2 = 1.5f;

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
            textMesh.gameObject.SetActive(true); 
            yield return new WaitForSeconds(time1);
            textMesh.gameObject.SetActive(false); 
            yield return new WaitForSeconds(time2);
        }
    }
}
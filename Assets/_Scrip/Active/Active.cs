using UnityEngine;

public class Active : MonoBehaviour
{
    public GameObject active;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            active.SetActive(!active.activeSelf);
        }
    }
}
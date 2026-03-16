using UnityEngine;

public class ActiveGame : MonoBehaviour
{
    public GameObject activebando;
    
    public void ToggleBando()
    {
        if (activebando != null)
        {
            activebando.SetActive(!activebando.activeSelf);
        }
    }
}

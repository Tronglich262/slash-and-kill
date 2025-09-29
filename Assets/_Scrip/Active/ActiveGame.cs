using UnityEngine;

public class ActiveGame : MonoBehaviour
{
    public GameObject activebando;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleBando()
    {
        if (activebando != null)
        {
            activebando.SetActive(!activebando.activeSelf);
        }
    }
}

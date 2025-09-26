using UnityEngine;

public class Main : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public delegate void thongbao();
    public event thongbao thongbaoload;
    void Start()
    {
        thongbaoload?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

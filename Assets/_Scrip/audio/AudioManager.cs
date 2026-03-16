using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider; 

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        AudioListener.volume = savedVolume;
        volumeSlider.value = savedVolume;
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("GameVolume", volume); // Lưu lại
        PlayerPrefs.Save();
    }
}

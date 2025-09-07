using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider; // Kéo thả slider vào đây trong Inspector

    void Start()
    {
        // Load volume đã lưu (nếu có), mặc định = 1 (100%)
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        AudioListener.volume = savedVolume;
        volumeSlider.value = savedVolume;

        // Gắn sự kiện khi kéo slider
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("GameVolume", volume); // Lưu lại
        PlayerPrefs.Save();
    }
}

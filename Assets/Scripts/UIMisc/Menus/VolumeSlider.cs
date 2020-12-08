using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public Audio Audio;
    private Slider _slider;

    private void Start()
    {
        _slider = GetComponent<Slider>();
        _slider.value = Audio.Volume;
    }

    public void OnChange()
    {
        Audio.Volume = _slider.value;
    }
}

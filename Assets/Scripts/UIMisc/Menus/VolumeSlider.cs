using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public Audio Audio;
    public Slider _slider;

    private void Start()
    {
        var slider = GetComponent<Slider>();
        if (slider != null)
        {
            _slider = slider;
        }
        _slider.value = Audio.Volume;
    }

    public void OnChange()
    {
        Audio.Volume = _slider.value;
    }
}

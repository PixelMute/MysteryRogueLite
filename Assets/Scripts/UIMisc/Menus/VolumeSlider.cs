using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public Audio Audio;
    public Slider _slider;

    private void Awake()
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
        
        if (_slider != null)
        {
            Audio.Volume = _slider.value;
        }
        
    }
}

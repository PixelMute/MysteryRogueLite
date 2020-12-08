using UnityEngine;

public class Audio : MonoBehaviour
{
    public string NameOfAudioSource;
    private AudioSource _audioSource;

    public float Volume
    {
        get
        {
            return _audioSource.volume;
        }
        set
        {
            _audioSource.volume = value;
            PlayerPrefs.SetFloat(NameOfAudioSource, value);
        }
    }
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (PlayerPrefs.HasKey(NameOfAudioSource))
        {
            _audioSource.volume = PlayerPrefs.GetFloat(NameOfAudioSource);
        }
        else
        {
            Volume = 1f;
        }
    }



}


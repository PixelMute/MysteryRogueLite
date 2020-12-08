using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource MusicSource;
    public AudioSource EffectSource;
    private Dictionary<string, AudioClip> _audioClips;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        _audioClips = new Dictionary<string, AudioClip>();
    }

    public static void PlayPlayerHit()
    {
        var rand = new System.Random();
        instance.PlaySoundEffect("man-hit-0" + rand.Next(1, 4));
    }

    public static void PlayEnemyHit()
    {
        instance.PlaySoundEffect("force-field-impact");
    }

    public static void PlayNextFloor()
    {
        instance.PlaySoundEffect("new-floor-01");
    }

    public static void PlayAlertNoise()
    {
        instance.PlaySoundEffect("enemy-alert-01");
    }

    public static void PlayPickMoney()
    {
        instance.PlaySoundEffect("bag-o-money-01");
    }

    public static void PlayGun()
    {
        instance.PlaySoundEffect("gun-shot-01");
    }

    public void PlaySoundEffect(string soundName)
    {
        if (_audioClips.ContainsKey(soundName))
        {
            EffectSource.PlayOneShot(_audioClips[soundName]);
        }
        else
        {
            var clip = Resources.Load<AudioClip>("Sounds/" + soundName);
            EffectSource.PlayOneShot(clip);
            _audioClips.Add(soundName, clip);
        }
    }

    public void PlayMusic(string soundName)
    {
        if (_audioClips.ContainsKey(soundName))
        {
            MusicSource.PlayOneShot(_audioClips[soundName]);
        }
        else
        {
            var clip = Resources.Load<AudioClip>("Sounds/" + soundName);
            MusicSource.PlayOneShot(clip);
            _audioClips.Add(soundName, clip);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioSource AudioSource;
    public AudioSource _audioSource;

    public void Start() {
        AudioSource = _audioSource;
    }

    public static void PlayPlayerHit()
    {
        var rand = new System.Random();
        PlayClip("man-hit-0" + rand.Next(1,4));
    }

    public static void PlayEnemyHit()
    {
        PlayClip("force-field-impact");
    }

    public static void PlayNextFloor()
    {
        PlayClip("new-floor-01");
    }

    public static void PlayAlertNoise()
    {
        PlayClip("enemy-alert-01");
    }

    public static void PlayPickMoney()
    {
        PlayClip("bag-o-money-01");
    }

    public static void PlayGun()
    {
        PlayClip("gun-shot-01");
    }

    private static void PlayClip(string soundName)
    {
        var clip = Resources.Load<AudioClip>("Sounds/" + soundName);
        AudioSource.PlayOneShot(clip);
    }
}

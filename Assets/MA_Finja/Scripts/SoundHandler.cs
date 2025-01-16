using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundHandler : MonoBehaviour
{
    public static SoundHandler instance;

    [SerializeField] private AudioSource soundObject;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void playSound(AudioClip audioClip, Transform spwanTransform, float volume)
    {
        AudioSource audioSource = Instantiate(soundObject, spwanTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }
}

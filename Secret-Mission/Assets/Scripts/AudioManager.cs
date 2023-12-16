using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource Backsound;
    [SerializeField] AudioSource SFX;

    public AudioClip BacksoundClip;
    public AudioClip Click;
    public AudioClip GunSound;

    private void Start()
    {
        Backsound.clip = BacksoundClip;
        Backsound.Play();
    }

    public void ClickSound()
    {
        SFX.clip = Click;
        SFX.Play();
    }

    public void PlaySFX(AudioClip Clip) 
    {
        SFX.PlayOneShot(Clip);
    }
}

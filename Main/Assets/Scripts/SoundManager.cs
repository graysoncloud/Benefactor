using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource efxSource;
    public AudioSource musicSource;
    public static SoundManager instance = null;

    public float lowPitchRange = .95f;
    public float highPitchRange = 1.05f;

    public AudioClip buttonPressed;

    public AudioClip takeDamage;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void PlaySingle (AudioClip clip)
    {
        efxSource.clip = clip;
        efxSource.Play();
    }

    public void RandomizeSfx (params AudioClip [] clips)
    {
        int randomIndex = Random.Range(0, clips.Length);
        float randomPitch = Random.Range(lowPitchRange, highPitchRange);

        efxSource.pitch = randomPitch;
        efxSource.clip = clips[randomIndex];
        efxSource.Play();
    }

    public void ButtonPress()
    {
        efxSource.clip = buttonPressed;
        efxSource.Play();
    }

    public void TakeDamage()
    {
        Debug.Log("PLAYING TAKE DAMAGE");
        efxSource.clip = takeDamage;
        efxSource.Play();
    }

}

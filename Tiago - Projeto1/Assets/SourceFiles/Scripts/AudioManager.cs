using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private AudioSource systemaSource;
    private List<AudioSource> activeSource;




    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            systemaSource = GetComponent<AudioSource>();
            activeSource = new List<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region AudioControls3D

    public void Play(AudioClip clip, AudioSource source)
    {
        if (!activeSource.Contains(source))
            activeSource.Add(source);
        systemaSource.Stop();
        systemaSource.clip = clip;
        systemaSource.Play();
    }

    public void Stop(AudioClip clip)
    {
        systemaSource.PlayOneShot(clip);
    }

    public void Stop(AudioSource source)
    {
        if (activeSource.Contains(source))
            activeSource.Remove(source);
        source.Stop();
        systemaSource.Stop();
    }

    public void Pause()
    {
        systemaSource.Pause();
    }

    public void Resume()
    {
        systemaSource.UnPause();
    }
#endregion



}
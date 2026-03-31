using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public List<AudioClip> audioClips = new List<AudioClip>();
    public int selectedIndex = 0;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlaySelected()
    {
        if (selectedIndex >= 0 && selectedIndex < audioClips.Count)
        {
            AudioManager.instance.Play(audioClips[selectedIndex], audioSource);
        }
    }

    public void Stop()
    {
        AudioManager.instance.Stop(audioSource);
    }

    public void Pause()
    {
        AudioManager.instance.Pause();
    }

    public void Resume()
    {
        AudioManager.instance.Resume();
    }
}

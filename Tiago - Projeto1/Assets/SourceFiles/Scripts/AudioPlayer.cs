using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField]
    private List<AudioClip> clips = new List<AudioClip>();

    [SerializeField]
    private int selectedIndex = 0;

    [Header("AudioSource Options")]
    [SerializeField]
    private List<AudioSource> audioSources = new List<AudioSource>();

    [SerializeField]
    private int selectedSourceIndex = 0;

    [SerializeField]
    private bool useAudioSource = false;

    public IReadOnlyList<AudioClip> Clips => clips;

    public int SelectedIndex
    {
        get => selectedIndex;
        set => selectedIndex = Mathf.Clamp(value, 0, Mathf.Max(0, clips.Count - 1));
    }

    // Helper to get the currently selected AudioSource (may be null)
    private AudioSource GetSelectedAudioSource()
    {
        if (!useAudioSource) return null;
        if (audioSources == null || audioSources.Count == 0) return null;
        if (selectedSourceIndex < 0 || selectedSourceIndex >= audioSources.Count) return null;
        return audioSources[selectedSourceIndex];
    }

    // Play the clip at index via AudioManager. If useAudioSource is true, an explicitly selected AudioSource
    // will be passed to AudioManager; otherwise the 2D/system API is used.
    public void Play(int index)
    {
        if (AudioManager.Instance == null) return;
        if (index < 0 || index >= clips.Count) return;

        AudioClip clip = clips[index];
        if (clip == null) return;

        if (useAudioSource)
        {
            AudioSource src = GetSelectedAudioSource();
            if (src == null)
            {
                Debug.LogWarning("AudioPlayer: 'useAudioSource' is true but no valid AudioSource is selected. Aborting Play().");
                return;
            }
            AudioManager.Instance.Play(clip, src);
        }
        else
        {
            AudioManager.Instance.Play(clip);
        }
    }

    public void Stop()
    {
        if (AudioManager.Instance == null) return;
        if (useAudioSource)
        {
            AudioSource src = GetSelectedAudioSource();
            if (src == null)
            {
                Debug.LogWarning("AudioPlayer: 'useAudioSource' is true but no valid AudioSource is selected. Aborting Stop().");
                return;
            }
            AudioManager.Instance.Stop(src);
        }
        else
        {
            AudioManager.Instance.Stop();
        }
    }

    public void Pause()
    {
        if (AudioManager.Instance == null) return;
        if (useAudioSource)
        {
            AudioSource src = GetSelectedAudioSource();
            if (src == null)
            {
                Debug.LogWarning("AudioPlayer: 'useAudioSource' is true but no valid AudioSource is selected. Aborting Pause().");
                return;
            }
            AudioManager.Instance.Pause(src);
        }
        else
        {
            AudioManager.Instance.Pause();
        }
    }

    public void Resume()
    {
        if (AudioManager.Instance == null) return;
        if (useAudioSource)
        {
            AudioSource src = GetSelectedAudioSource();
            if (src == null)
            {
                Debug.LogWarning("AudioPlayer: 'useAudioSource' is true but no valid AudioSource is selected. Aborting Resume().");
                return;
            }
            AudioManager.Instance.Resume(src);
        }
        else
        {
            AudioManager.Instance.Resume();
        }
    }
}
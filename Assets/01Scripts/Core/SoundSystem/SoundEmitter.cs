using System.Collections;
using System.Collections.Generic;
using PJH.Utility.Extensions;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
    public SoundDataSO DataSO { get; private set; }
    public LinkedListNode<SoundEmitter> Node { get; set; }

    AudioSource audioSource;
    Coroutine playingCoroutine;

    void Awake()
    {
        audioSource = gameObject.GetOrAdd<AudioSource>();
    }

    public void Initialize(SoundDataSO dataSO)
    {
        DataSO = dataSO;
        audioSource.clip = dataSO.clip;
        audioSource.outputAudioMixerGroup = dataSO.mixerGroup;
        audioSource.loop = dataSO.loop;
        audioSource.playOnAwake = dataSO.playOnAwake;

        audioSource.mute = dataSO.mute;
        audioSource.bypassEffects = dataSO.bypassEffects;
        audioSource.bypassListenerEffects = dataSO.bypassListenerEffects;
        audioSource.bypassReverbZones = dataSO.bypassReverbZones;

        audioSource.priority = dataSO.priority;
        audioSource.volume = dataSO.volume;
        audioSource.pitch = dataSO.pitch;
        audioSource.panStereo = dataSO.panStereo;
        audioSource.spatialBlend = dataSO.spatialBlend;
        audioSource.reverbZoneMix = dataSO.reverbZoneMix;
        audioSource.dopplerLevel = dataSO.dopplerLevel;
        audioSource.spread = dataSO.spread;

        audioSource.minDistance = dataSO.minDistance;
        audioSource.maxDistance = dataSO.maxDistance;

        audioSource.ignoreListenerVolume = dataSO.ignoreListenerVolume;
        audioSource.ignoreListenerPause = dataSO.ignoreListenerPause;

        audioSource.rolloffMode = dataSO.rolloffMode;
    }

    public void Play()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
        }

        audioSource.Play();
        playingCoroutine = StartCoroutine(WaitForSoundToEnd());
    }

    IEnumerator WaitForSoundToEnd()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        Stop();
    }

    public void Stop()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
            playingCoroutine = null;
        }

        audioSource.Stop();
        SoundManager.Instance.ReturnToPool(this);
    }

    public void WithRandomPitch(float min = -0.05f, float max = 0.05f)
    {
        audioSource.pitch += Random.Range(min, max);
    }
}
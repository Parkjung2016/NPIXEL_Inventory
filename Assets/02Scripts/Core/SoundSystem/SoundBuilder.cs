using PJH.Utility;
using UnityEngine;

public struct SoundBuilder
{
    readonly SoundManager soundManager;
    Vector3 position;
    bool randomPitch;

    public SoundBuilder(SoundManager soundManager, Vector3 position = default, bool randomPitch = false)
    {
        this.soundManager = soundManager;
        this.position = position;
        this.randomPitch = randomPitch;
    }

    public SoundBuilder WithPosition(Vector3 position)
    {
        this.position = position;
        return this;
    }

    public SoundBuilder WithRandomPitch()
    {
        this.randomPitch = true;
        return this;
    }

    public SoundEmitter Play(SoundDataSO soundDataSO)
    {
        if (soundDataSO == null)
        {
            PJHDebug.LogError("SoundData is null", tag: "SoundBuilder");
            return null;
        }

        if (!soundManager.CanPlaySound(soundDataSO)) return null;

        SoundEmitter soundEmitter = soundManager.Get();
        soundEmitter.Initialize(soundDataSO);
        soundEmitter.transform.position = position;
        soundEmitter.transform.parent = soundManager.transform;

        if (randomPitch)
        {
            soundEmitter.WithRandomPitch();
        }

        if (soundDataSO.frequentSound)
        {
            soundEmitter.Node = soundManager.frequentSoundEmitters.AddLast(soundEmitter);
        }

        soundEmitter.Play();
        return soundEmitter;
    }
}
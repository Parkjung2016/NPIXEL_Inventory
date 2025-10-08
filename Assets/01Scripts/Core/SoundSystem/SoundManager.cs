using System.Collections.Generic;
using PJH.Utility;
using PJH.Utility.Singleton;
using UnityEngine;
using UnityEngine.Pool;

public class SoundManager : PersistentSingleton<SoundManager>
{
    private IObjectPool<SoundEmitter> _soundEmitterPool;
    private readonly List<SoundEmitter> activeSoundEmitters = new();
    public readonly LinkedList<SoundEmitter> frequentSoundEmitters = new();

    [SerializeField] SoundEmitter soundEmitterPrefab;
    [SerializeField] bool collectionCheck = true;
    [SerializeField] int defaultCapacity = 10;
    [SerializeField] int maxPoolSize = 100;
    [SerializeField] int maxSoundInstances = 30;

    private void Start()
    {
        InitializePool();
    }

    public static SoundBuilder CreateSoundBuilder() => new SoundBuilder(Instance);

    public bool CanPlaySound(SoundDataSO dataSO)
    {
        if (!dataSO.frequentSound) return true;

        if (frequentSoundEmitters.Count >= maxSoundInstances)
        {
            try
            {
                frequentSoundEmitters.First.Value.Stop();
                return true;
            }
            catch
            {
                PJHDebug.Log("SoundEmitter is already released", tag: "SoundManager");
            }

            return false;
        }

        return true;
    }

    public SoundEmitter Get()
    {
        return _soundEmitterPool.Get();
    }

    public void ReturnToPool(SoundEmitter soundEmitter)
    {
        _soundEmitterPool.Release(soundEmitter);
    }

    public static void StopAll()
    {
        foreach (var soundEmitter in Instance.activeSoundEmitters)
        {
            soundEmitter.Stop();
        }

        Instance.frequentSoundEmitters.Clear();
    }

    private void InitializePool()
    {
        _soundEmitterPool = new ObjectPool<SoundEmitter>(
            CreateSoundEmitter,
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyPoolObject,
            collectionCheck,
            defaultCapacity,
            maxPoolSize);
    }

    private SoundEmitter CreateSoundEmitter()
    {
        var soundEmitter = Instantiate(soundEmitterPrefab);
        soundEmitter.gameObject.SetActive(false);
        return soundEmitter;
    }

    private void OnTakeFromPool(SoundEmitter soundEmitter)
    {
        soundEmitter.gameObject.SetActive(true);
        activeSoundEmitters.Add(soundEmitter);
    }

    private void OnReturnedToPool(SoundEmitter soundEmitter)
    {
        if (soundEmitter.Node != null)
        {
            frequentSoundEmitters.Remove(soundEmitter.Node);
            soundEmitter.Node = null;
        }

        soundEmitter.gameObject.SetActive(false);
        activeSoundEmitters.Remove(soundEmitter);
    }

    private void OnDestroyPoolObject(SoundEmitter soundEmitter)
    {
        Destroy(soundEmitter.gameObject);
    }
}
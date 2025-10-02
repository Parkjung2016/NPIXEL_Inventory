using System;
using System.Collections.Generic;
using System.IO;
using PJH.Utility;
using UnityEngine;

[CreateAssetMenu]
public class SaveManagerSO : ScriptableObject
{
    public event Action OnSaveCompleted;
    public event Action OnLoadCompleted;
    private List<ISaveable> _saveables = new List<ISaveable>();

    private void OnEnable()
    {
        _saveables.Clear();
    }

    public async void Save()
    {
        _saveables.Sort(CompareID);
        for (int i = 0; i < _saveables.Count; i++)
        {
            byte[] bytes = await _saveables[i].ParsingToBytes();
            string path = GetSaveFilePath(_saveables[i].SaveID);
            File.WriteAllBytes(path, bytes);
        }

        PJHDebug.LogColorPart("Game Saved", Color.green, tag: "SaveManagerSO");
        OnSaveCompleted?.Invoke();
    }

    public int CompareID(ISaveable saveable, ISaveable saveable2)
    {
        return saveable2.SaveID.CompareTo(saveable.SaveID); // 내림차순
    }

    public async void Load()
    {
        _saveables.Sort(CompareID);
        for (int i = 0; i < _saveables.Count; i++)
        {
            string path = GetSaveFilePath(_saveables[i].SaveID);
            byte[] bytes = File.ReadAllBytes(path);
            await _saveables[i].ParsingFromBytes(bytes);
        }

        PJHDebug.LogColorPart("Game Loaded", Color.green, tag: "SaveManagerSO");

        for (int i = 0; i < _saveables.Count; i++)
        {
            _saveables[i].AllLoaded();
        }

        OnLoadCompleted?.Invoke();
    }

    private string GetSaveFilePath(int saveID)
    {
        return Path.Combine(Application.persistentDataPath, $"SaveData_{saveID}.dat");
    }

    public void RegisterSaveable(ISaveable saveable)
    {
        if (!_saveables.Contains(saveable))
        {
            _saveables.Add(saveable);
        }
    }

    public void UnregisterSaveable(ISaveable saveable)
    {
        if (_saveables.Contains(saveable))
        {
            _saveables.Remove(saveable);
        }
    }
}
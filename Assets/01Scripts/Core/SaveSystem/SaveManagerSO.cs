using System;
using System.Collections.Generic;
using System.IO;
using PJH.Utility;
using UnityEngine;

[CreateAssetMenu]
public class SaveManagerSO : ScriptableObject
{
    private List<ISaveable> _saveables = new List<ISaveable>();

    private void OnEnable()
    {
        _saveables.Clear();
    }

    public async void Save(Action OnSaveCompleted = null)
    {
        for (int i = 0; i < _saveables.Count; i++)
        {
            byte[] bytes = await _saveables[i].ParsingToBytes();
            string path = GetSaveFilePath(_saveables[i].SaveID);
            File.WriteAllBytes(path, bytes);
        }

        PJHDebug.LogColorPart("Game Saved", Color.green, tag: "SaveManagerSO");
        OnSaveCompleted?.Invoke();
    }

    public async void Load(Action OnLoadCompleted = null)
    {
        for (int i = 0; i < _saveables.Count; i++)
        {
            string path = GetSaveFilePath(_saveables[i].SaveID);
            byte[] bytes = File.ReadAllBytes(path);
            await _saveables[i].ParsingFromBytes(bytes);
        }

        PJHDebug.LogColorPart("Game Loaded", Color.green, tag: "SaveManagerSO");

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
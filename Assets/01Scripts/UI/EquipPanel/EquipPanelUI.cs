using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

public class EquipPanelUI : UIBase
{
    enum Objects
    {
        StatTextGroup
    }

    [SerializeField] private StatText _statTextPrefab;
    [Inject] private PlayerStatus _playerStatus;
    [Inject] private ItemManagerSO _itemManagerSO;

    private Dictionary<string, StatText> _statTextDictionary = new Dictionary<string, StatText>();

    public override void Init()
    {
        Bind<GameObject>(typeof(Objects));
        Transform statTextGroup = GetObject((byte)Objects.StatTextGroup).transform;
        _playerStatus.OnLoadedPlayerStatusData += HandleLoadedPlayerStatusData;

        for (int i = 0; i < _playerStatus.playerStatusData.statDatas.Length; i++)
        {
            StatData statData = _playerStatus.playerStatusData.statDatas[i];
            StatText statText = Instantiate(_statTextPrefab, statTextGroup);
            statText.BindStat(statData);
            _statTextDictionary.Add(statData.statName, statText);
        }
    }

    protected override void OnDestroy()
    {
        _playerStatus.OnLoadedPlayerStatusData -= HandleLoadedPlayerStatusData;
    }

    private void HandleLoadedPlayerStatusData(PlayerStatusData playerStatusData)
    {
        foreach (var pair in playerStatusData.equippedItems)
        {
            Debug.Log(pair.Value);
            _itemManagerSO.EquipItemForce(pair.Value);
        }

        for (int i = 0; i < _playerStatus.playerStatusData.statDatas.Length; i++)
        {
            StatData statData = _playerStatus.playerStatusData.statDatas[i];
            StatText statText = _statTextDictionary[statData.statName];
            statText.BindStat(statData);
        }
    }
}
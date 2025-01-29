using Game.Common.Packets;
using Game.Packets;
using LiteNetLib;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(ServerEntity))]
public class ManaSync : MonoBehaviour
{
    private ServerEntity _serverEntity;

    [SerializeField] private TMP_Text _maxManaText;
    [SerializeField] private TMP_Text _currentManaText;
    [SerializeField] private Slider _manaSlider;

    private void Awake()
    {
        NetworkManager.Instance.PacketDispatcher.Subscribe<EntityManaChangedPacket>(OnEntityManaChangedPacketRecieved);
        _serverEntity = GetComponent<ServerEntity>();
    }

    private void OnEntityManaChangedPacketRecieved(NetPeer peer, EntityManaChangedPacket packet)
    {
        if (packet.EntityID != _serverEntity.EntityID)
            return;

        if (_maxManaText != null)
        {
            _maxManaText.text = packet.MaxValue.ToString();
        }

        if (_currentManaText != null)
        {
            _currentManaText.text = packet.CurrentValue.ToString();
        }

        if (_manaSlider != null)
        {
            _manaSlider.maxValue = packet.MaxValue;
            _manaSlider.value = packet.CurrentValue;
        }
    }

    private void OnDestroy()
    {
        NetworkManager.Instance.PacketDispatcher.Unsubscribe<EntityManaChangedPacket>(OnEntityManaChangedPacketRecieved);
    }
}

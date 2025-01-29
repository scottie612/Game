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
public class HealthSync : MonoBehaviour
{
    private ServerEntity _serverEntity;

    [SerializeField] private TMP_Text _maxHealthText;
    [SerializeField] private TMP_Text _currentHealthText;
    [SerializeField] private Slider _healthSlider;

    private void Awake()
    {
        NetworkManager.Instance.PacketDispatcher.Subscribe<EntityHealthChangedPacket>(OnEntityHealthChangedPacketRecieved);
        _serverEntity = GetComponent<ServerEntity>();
    }

    private void OnEntityHealthChangedPacketRecieved(NetPeer peer, EntityHealthChangedPacket packet)
    {
        if (packet.EntityID != _serverEntity.EntityID)
            return;

        if (_maxHealthText != null)
        {
            _maxHealthText.text = packet.MaxValue.ToString();
        }

        if (_currentHealthText != null)
        {
            _currentHealthText.text = packet.CurrentValue.ToString();
        }

        if (_healthSlider != null)
        {
            _healthSlider.maxValue = packet.MaxValue;
            _healthSlider.value = packet.CurrentValue;
        }

    }

    private void OnDestroy()
    {
        NetworkManager.Instance.PacketDispatcher.Unsubscribe<EntityHealthChangedPacket>(OnEntityHealthChangedPacketRecieved);
    }
}

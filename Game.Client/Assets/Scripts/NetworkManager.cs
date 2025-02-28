using Game.Common;
using Game.Common.Encryption;
using Game.Common.Enums;
using Game.Common.Packets.Interfaces;
using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : Singleton<NetworkManager>, INetEventListener
{
    private NetManager _netManager;
    private NetDataWriter _writer = new NetDataWriter();

    public PacketDispatcher PacketDispatcher { get; private set; } = new PacketDispatcher();

    private void Start()
    {
        _netManager = new NetManager(this);
        _netManager.IPv6Enabled = false;
        _netManager.DisconnectTimeout = 300000;
        _netManager.Start();

        //Build AuthData
        var authData = new AuthData()
        {
            PlayFabId = Globals.PlayFabUserID,
            SessionTicket = Globals.SessionTicket,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ProtocolVersion = 1,
            DeviceFingerprint = "DeviceFingerprint",
            Nonce = Guid.NewGuid().ToString(),
        };

        var publicKey = EncryptionHelper.GetPublicKey(Globals.RSAKeypair);

        //Generate AES Key
        var aesKey = EncryptionHelper.GenerateAesKey();

        //Encrypt AES Key with Server Public Key
        var encryptedAesKey = EncryptionHelper.Encrypt(Globals.ServerPublicKey, aesKey);

        //Encrypt AuthData with AES Key
        var encryptedAuthData = EncryptionHelper.EncryptAes(aesKey, JsonConvert.SerializeObject(authData));

        //Sign AuthData with Private Key
        var signature = EncryptionHelper.Sign(EncryptionHelper.GetPrivateKey(Globals.RSAKeypair), JsonConvert.SerializeObject(authData));

        _writer.Put(publicKey);
        _writer.Put(encryptedAesKey);
        _writer.Put(encryptedAuthData);
        _writer.Put(signature);

        _netManager.Connect(Globals.ServerIP, 7777, _writer);
        _writer.Reset();
    }

    private void FixedUpdate()
    {
        _netManager?.PollEvents();
        PacketDispatcher.SendAllPackets(_netManager, _writer);
    }

    protected override void OnApplicationQuit()
    {
        _netManager.DisconnectAll();
        _netManager.Stop();
        base.OnApplicationQuit();
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("Connected!");
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("Disconnected!");
        SceneManager.LoadScene("Play");
    }

    public void Disconnect()
    {
        _netManager.DisconnectAll();
        SceneManager.LoadScene("Play");
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        PacketType type = (PacketType)reader.GetByte();

        List<IPacket> packets = PacketDispatcher.Deserialize(type, reader);
        if (packets.Count != 0)
        {
            foreach (IPacket packet in packets)
            {
                PacketDispatcher.RaiseEvent(peer, packet);
            }
        }
        else
        {
            Debug.LogError("Packet Type not found or failed to deserialize packet");
        }
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {

    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {

    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {

    }

    public void OnConnectionRequest(ConnectionRequest request)
    {

    }
}

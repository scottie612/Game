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

public class NetworkManager : Singleton<NetworkManager>, INetEventListener
{

    [SerializeField] private string ip;
    [SerializeField] private ushort port;

    private NetManager _netManager;
    private NetDataWriter _writer = new NetDataWriter();

    public PacketDispatcher PacketDispatcher { get; private set; } = new PacketDispatcher();

    private void Start()
    {
        _netManager = new NetManager(this);
        _netManager.IPv6Enabled = false;
        _netManager.DisconnectTimeout = 300000;
        _netManager.Start();



        //Get Server Public Key
        var publicKeyRequest = new GetTitleDataRequest()
        {
            Keys = new List<string>() { "PublicKey" }
        };

        var serverPublicKey = string.Empty;
        PlayFabClientAPI.GetTitleData(publicKeyRequest, result =>
        {
            result.Data.TryGetValue("PublicKey", out serverPublicKey);

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

            //Generate AES Key
            var aesKey = EncryptionHelper.GenerateAesKey();

            //Encrypt AES Key with Server Public Key
            var encryptedAesKey = EncryptionHelper.Encrypt(serverPublicKey, aesKey);

            //Encrypt AuthData with AES Key
            var encryptedAuthData = EncryptionHelper.EncryptAes(aesKey, JsonConvert.SerializeObject(authData));

            //Sign AuthData with Private Key
            var signature = EncryptionHelper.Sign(EncryptionHelper.GetPrivateKey(), JsonConvert.SerializeObject(authData));

            _writer.Put(encryptedAesKey);
            _writer.Put(encryptedAuthData);
            _writer.Put(signature);
            
            _netManager.Connect(ip, port, _writer);
            _writer.Reset();

        }, error =>
        {
            Debug.LogError(error.ErrorMessage);
        });

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

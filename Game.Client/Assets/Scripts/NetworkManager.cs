using Game.Configuration;
using Game.Packets;
using Game.Events;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetworkManager : Singleton<NetworkManager>, INetEventListener
{

    [SerializeField] private string ip;
    [SerializeField] private ushort port;

    private NetManager _netManager;
    private NetDataWriter _writer = new NetDataWriter();
    private NetPacketProcessor _packetProcessor = new NetPacketProcessor();

    private void Start()
    {
        _netManager = new NetManager(this);
        _netManager.IPv6Enabled = false;
        _netManager.Start();
        _netManager.Connect(ip, port, "");
    }

    private void FixedUpdate()
    {
        _netManager?.PollEvents();
    }

    public void Send<T>(T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
    {
        _writer.Reset();
        packet.Serialize(_writer);
        _netManager.FirstPeer.Send(_writer, deliveryMethod);
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
        var packetType = (Packet)reader.GetByte();
        switch (packetType)
        {
            case Packet.Identity:
                var identityPacket = new IdentityPacket();
                identityPacket.Deserialize(reader);
                EntityEventManager<IdentityPacket>.RaiseEvent(identityPacket);
                break;
            case Packet.EntitySpawned:
                var entitySpawnedPacket = new EntitySpawnedPacket();
                entitySpawnedPacket.Deserialize(reader);
                EntityEventManager<EntitySpawnedPacket>.RaiseEvent(entitySpawnedPacket);
                break;
            case Packet.EntityDespawned:
                var entityDespawnedPacket = new EntityDespawnedPacket();
                entityDespawnedPacket.Deserialize(reader);
                EntityEventManager<EntityDespawnedPacket>.RaiseEvent(entityDespawnedPacket);
                break;
            case Packet.EntityMovement:
                var numberOfUpdates = reader.GetUShort();
                for (int i = 0; i < numberOfUpdates; i++)
                {
                    var movementData = new EntityMovementData();
                    movementData.Deserialize(reader);
                    EntityEventManager<EntityMovementData>.RaiseEvent(movementData);
                }
                break;
            default:
                break;
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

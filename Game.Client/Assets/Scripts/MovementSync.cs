using Game.Packets;
using LiteNetLib;
using UnityEngine;

[RequireComponent(typeof(ServerEntity))]
public class MovementSync : MonoBehaviour
{
    private ServerEntity _serverEntity;

    private Vector3 _newPosition;
    private void Start()
    {
        _newPosition = transform.position;
        _serverEntity = GetComponent<ServerEntity>();
        NetworkManager.Instance.PacketDispatcher.Subscribe<EntityMovementPacket>(OnEntityMovementPacketRecieved);
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, _newPosition, Time.deltaTime * 10);
    }

    public void OnEntityMovementPacketRecieved(NetPeer peer, EntityMovementPacket packet)
    {
        if(packet.EntityID != _serverEntity.EntityID)
            return;
        _newPosition = new Vector3(packet.Position.X, packet.Position.Y, 0);
    }

    private void OnDestroy()
    {
        NetworkManager.Instance.PacketDispatcher.Unsubscribe<EntityMovementPacket>(OnEntityMovementPacketRecieved);
    }
}

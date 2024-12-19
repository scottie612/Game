using Game.Packets;
using Game.Events;
using UnityEngine;

[RequireComponent(typeof(ServerEntity))]
public class MovementSync : MonoBehaviour, IEntityEventListener<EntityMovementData>
{
    private ServerEntity _serverEntity;

    private Vector3 _newPosition;
    private void Start()
    {
        _newPosition = transform.position;
        _serverEntity = GetComponent<ServerEntity>();
        EntityEventManager<EntityMovementData>.Subscribe(_serverEntity.EntityID, this);
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, _newPosition, Time.deltaTime * 10);
    }

    public void OnEvent(EntityMovementData packet)
    {
        _newPosition = new Vector3(packet.Position.X, packet.Position.Y, 0);
    }

    private void OnDestroy()
    {
        EntityEventManager<EntityMovementData>.Unsubscribe(_serverEntity.EntityID, this);
    }
}

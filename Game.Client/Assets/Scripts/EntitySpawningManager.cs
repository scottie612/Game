using Game.Configuration;
using Game.Packets;
using Game.Events;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawningManager : Singleton<EntitySpawningManager>, IEntityEventListener<EntitySpawnedPacket>, IEntityEventListener<EntityDespawnedPacket>
{
    public ServerEntity PlayerPrefab;
    public ServerEntity FireballPrefab;


    public Dictionary<int, GameObject> SpawnedEntites = new Dictionary<int, GameObject>();

    void Start()
    {
        EntityEventManager<EntitySpawnedPacket>.Subscribe(this);
        EntityEventManager<EntityDespawnedPacket>.Subscribe(this);
    }

    public void OnEvent(EntitySpawnedPacket packet)
    {
        ServerEntity spawnedEntity = null;
        switch (packet.Type)
        {
            case EntityType.Player:
                spawnedEntity = Instantiate(PlayerPrefab, new Vector3(packet.StartingX, packet.StartingY, 0), Quaternion.identity);
                spawnedEntity.EntityID = packet.EntityID;
                break;
            case EntityType.FireBall:
                spawnedEntity = Instantiate(FireballPrefab);
                spawnedEntity.transform.position = new Vector3( packet.StartingX, packet.StartingY, 0);
                spawnedEntity.EntityID = packet.EntityID;
                break;
            default:
                Debug.Log("Entity Type not implemented for spawnning");
                break;
        }

        SpawnedEntites?.Add(spawnedEntity.EntityID, spawnedEntity.gameObject);
    }

    public void OnEvent(EntityDespawnedPacket packet)
    {
        if (SpawnedEntites.TryGetValue(packet.EntityID, out GameObject spawnedEntity))
        {
            GameObject.Destroy(spawnedEntity.gameObject);
            SpawnedEntites?.Remove(packet.EntityID);
        }
    }

    void OnDestroy()
    {
        EntityEventManager<EntitySpawnedPacket>.Unsubscribe(this);
        EntityEventManager<EntityDespawnedPacket>.Unsubscribe(this);
    }


}

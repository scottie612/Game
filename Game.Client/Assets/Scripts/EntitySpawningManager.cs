using Game.Common.Enums;
using Game.Packets;
using LiteNetLib;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawningManager : Singleton<EntitySpawningManager>
{
    public ServerEntity PlayerPrefab;
    public ServerEntity FireballPrefab;


    public Dictionary<int, GameObject> SpawnedEntites = new Dictionary<int, GameObject>();

    void Start()
    {
        NetworkManager.Instance.PacketDispatcher.Subscribe<EntitySpawnedPacket>(OnEntitySpawnedPacketRecieved);
        NetworkManager.Instance.PacketDispatcher.Subscribe<EntityDespawnedPacket>(OnEntityDespawnedPacketRecieved);
    }

    public void OnEntitySpawnedPacketRecieved(NetPeer peer, EntitySpawnedPacket packet)
    {
        Debug.Log($"EntitySpawnedPacket Recieved! PacketType: {packet.Type}, EntityID:{packet.EntityID}, EntityName: {packet.EntityName}");
        ServerEntity spawnedEntity = null;
        switch (packet.Type)
        {
            case EntityType.Player:
                spawnedEntity = Instantiate(PlayerPrefab, new Vector3(packet.StartingX, packet.StartingY, 0), Quaternion.identity);
                spawnedEntity.EntityID = packet.EntityID;
                spawnedEntity.EntityName = packet.EntityName;

                // If the entity is the local player, Bind the camera to it
                if(spawnedEntity.EntityName == Globals.PlayFabUsername)
                {
                   spawnedEntity.gameObject.AddComponent<CameraFollowServerEntity>();
                }

                break;
            case EntityType.FireBall:
                spawnedEntity = Instantiate(FireballPrefab, new Vector3(packet.StartingX, packet.StartingY, 0), Quaternion.identity);
                spawnedEntity.EntityID = packet.EntityID;
                spawnedEntity.EntityName = packet.EntityName;
                break;
            case EntityType.Bullet:
                spawnedEntity = Instantiate(FireballPrefab, new Vector3(packet.StartingX, packet.StartingY, 0), Quaternion.identity);
                spawnedEntity.EntityID = packet.EntityID;
                spawnedEntity.EntityName = packet.EntityName;
                break;
            default:
                Debug.Log("Entity Type not implemented for spawnning");
                break;
        }
            
        if(spawnedEntity != null)
        {
            SpawnedEntites.Add(spawnedEntity.EntityID, spawnedEntity.gameObject);
        }
        
    }

    public void OnEntityDespawnedPacketRecieved(NetPeer peer, EntityDespawnedPacket packet)
    {
        if (SpawnedEntites.TryGetValue(packet.EntityID, out GameObject spawnedEntity))
        {
            GameObject.Destroy(spawnedEntity.gameObject);
            SpawnedEntites?.Remove(packet.EntityID);
        }
    }

    void OnDestroy()
    {
        NetworkManager.Instance.PacketDispatcher.Unsubscribe<EntitySpawnedPacket>(OnEntitySpawnedPacketRecieved);
        NetworkManager.Instance.PacketDispatcher.Unsubscribe<EntityDespawnedPacket>(OnEntityDespawnedPacketRecieved);
    }


}

using Cinemachine;
using Game.Packets;
using LiteNetLib;
using System.Collections;
using UnityEngine;


[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraFollowServerEntity : MonoBehaviour
{
    private CinemachineVirtualCamera _camera;

    void Start()
    {
        _camera = GetComponent<CinemachineVirtualCamera>();
        NetworkManager.Instance.PacketDispatcher.Subscribe<IdentityPacket>(OnIdentityPacketRecieved);
 
    }

    public void OnIdentityPacketRecieved(NetPeer peer, IdentityPacket eventPacket)
    {
        StartCoroutine(BindCameraToEntity(eventPacket.EntityID));
    }

    IEnumerator BindCameraToEntity(int EntityID)
    {
        GameObject followTarget;
        Debug.Log($"Looking for entity ID {EntityID}");
        while (!EntitySpawningManager.Instance.SpawnedEntites.TryGetValue(EntityID, out followTarget))
        {
            Debug.Log("Local Player not found");
            yield return null;
        }

        _camera.Follow = followTarget.transform;
    }
    private void OnDisable()
    {
        NetworkManager.Instance.PacketDispatcher.Unsubscribe<IdentityPacket>(OnIdentityPacketRecieved);
    }
}

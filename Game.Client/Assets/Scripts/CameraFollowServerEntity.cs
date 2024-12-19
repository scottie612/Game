using Cinemachine;
using Game.Packets;
using Game.Events;
using System.Collections;
using UnityEngine;


[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraFollowServerEntity : MonoBehaviour, IEntityEventListener<IdentityPacket>
{
    private CinemachineVirtualCamera _camera;

    void OnEnable()
    {
        _camera = GetComponent<CinemachineVirtualCamera>();
        EntityEventManager<IdentityPacket>.Subscribe(this);
    }
    public void OnEvent(IdentityPacket eventPacket)
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
        EntityEventManager<IdentityPacket>.Unsubscribe(this);
    }


}

using Cinemachine;
using Game.Packets;
using LiteNetLib;
using System.Collections;
using UnityEngine;

public class CameraFollowServerEntity : MonoBehaviour
{
    void Start()
    {
        var child = Camera.main.transform.GetChild(0);
        Debug.Log("Child Name: " + child.name);
        
        if(child.TryGetComponent<CinemachineVirtualCamera>(out var virtualCamera))
        {
            virtualCamera.Follow = gameObject.transform;
        }
        else
        {
            Debug.LogWarning("Camera does not have virtual camera component attached.");
        }
    }
}

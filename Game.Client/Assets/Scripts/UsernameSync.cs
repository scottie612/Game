using Game.Common.Packets;
using LiteNetLib;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(ServerEntity))]
public class UsernameSync : MonoBehaviour
{
    [SerializeField] private ServerEntity _serverEntity;

    [SerializeField] private TMP_Text _usernameText;

    [SerializeField] private Canvas _canvas;

    private Camera _mainCamera;
    

    private void Start()
    {
        _mainCamera = Camera.main;

        _canvas.worldCamera = _mainCamera;

        _serverEntity = GetComponent<ServerEntity>();
   
        _usernameText.text = _serverEntity.EntityName;
    }
}

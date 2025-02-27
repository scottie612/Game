using Game.Common.Packets;
using Game.Packets;
using LiteNetLib;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    
    private Vector2 _currentMovementInput;
    private Vector2 _previousMovementInput;

   
    void Update()
    {
        _currentMovementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (_previousMovementInput != _currentMovementInput)
        {
            var packet = new MovementRequestPacket();
            packet.InputVector = new System.Numerics.Vector2(_currentMovementInput.x, _currentMovementInput.y);

            NetworkManager.Instance.PacketDispatcher.Enqueue(packet);

            _previousMovementInput = _currentMovementInput;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0)) 
        {
            var mousePos = Input.mousePosition;
            mousePos.x -= Screen.width / 2;
            mousePos.y -= Screen.height / 2;

            mousePos = mousePos.normalized;

            var packet = new ActionRequestPacket();
            packet.CastDirection = new System.Numerics.Vector2(mousePos.x,mousePos.y);
            NetworkManager.Instance.PacketDispatcher.Enqueue(packet);

        }

        if (Input.GetKeyUp(KeyCode.Alpha1)) 
        { 
            var packet = new ChangeSelectedHotbarIndexRequestPacket();
            packet.Index = 0;
            NetworkManager.Instance.PacketDispatcher.Enqueue(packet);
            HotbarManager.Instance.ChangeSelectedHotbarIndex(0);

        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            var packet = new ChangeSelectedHotbarIndexRequestPacket();
            packet.Index = 1;
            NetworkManager.Instance.PacketDispatcher.Enqueue(packet);
            HotbarManager.Instance.ChangeSelectedHotbarIndex(1);
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            var packet = new ChangeSelectedHotbarIndexRequestPacket();
            packet.Index = 2;
            NetworkManager.Instance.PacketDispatcher.Enqueue(packet);
            HotbarManager.Instance.ChangeSelectedHotbarIndex(2);
        }
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            var packet = new ChangeSelectedHotbarIndexRequestPacket();
            packet.Index = 3;
            NetworkManager.Instance.PacketDispatcher.Enqueue(packet);
            HotbarManager.Instance.ChangeSelectedHotbarIndex(3);
        }
    }
}

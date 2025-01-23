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
            packet.XComponent = _currentMovementInput.x;
            packet.YComponent = _currentMovementInput.y;

            NetworkManager.Instance.PacketDispatcher.Enqueue(packet);

            _previousMovementInput = _currentMovementInput;
        }

        // NOT USED
        if (Input.GetKeyDown(KeyCode.Alpha1)) 
        {
            var mousePos = Input.mousePosition;
            mousePos.x -= Screen.width / 2;
            mousePos.y -= Screen.height / 2;

            mousePos = mousePos.normalized;

            var packet = new ActionRequestPacket();
            packet.AbilityIndex = 0;
            packet.CastDirection = new System.Numerics.Vector2(mousePos.x,mousePos.y);
            NetworkManager.Instance.PacketDispatcher.Enqueue(packet);

        }

    }
}

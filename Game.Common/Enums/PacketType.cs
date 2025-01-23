namespace Game.Common.Enums
{ 
    public enum PacketType : ushort
    {
        //Client
        MovementRequest = 1,
        ActionRequest,


        //Server
        Identity,
        EntitySpawned,
        EntityDespawned,
        EntityMovement,
    }
}

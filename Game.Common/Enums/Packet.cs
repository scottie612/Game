namespace Game.Configuration
{ 
    public enum Packet : ushort
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

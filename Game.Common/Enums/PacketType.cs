namespace Game.Common.Enums
{ 
    public enum PacketType : ushort
    {
        //Client
        MovementRequest = 1,
        ActionRequest,
        ChangeSelectedHotbarIndexRequest,


        //Server
        EntitySpawned,
        EntityDespawned,
        EntityMovement,
        EntityHealthChanged,
        EntityManaChanged,
        EntityDied,
        EntityAttacked,
    }
}

namespace Game.Events
{
    public interface IEntityEventListener<T> where T : IEntityEvent
    {
        void OnEvent(T eventPacket);
    }

}

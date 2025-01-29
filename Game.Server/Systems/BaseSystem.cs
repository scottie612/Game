using Game.Common;

namespace Game.Server.Systems
{
    public abstract class SystemBase
    {
        public GameWorld World { get; set; }
        public PacketDispatcher PacketDispatcher { get; set; }
        public SystemBase(GameWorld world, PacketDispatcher packetDispatcher)
        {
            World = world;
            PacketDispatcher = packetDispatcher;
            World.OnInitialize += Initialize;
            World.OnUpdate += Update;
            World.OnShutdown += Shutdown;
        }
        public virtual void Initialize()
        {

        }

        public virtual void Update(float deltaTime)
        {

        }

        public virtual void Shutdown()
        {

        }
    }
}

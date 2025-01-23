using Arch.Core;
using System;

namespace Game.Server
{
    public class GameWorld
    {
        public Action? OnInitialize;
        public Action<float>? OnUpdate;
        public Action? OnShutdown;
        public World World { get; set; }
        public GameWorld() 
        {
            World = World.Create();
        }
    }
}

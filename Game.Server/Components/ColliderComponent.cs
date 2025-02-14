using Arch.Core;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Game.Server.Components
{
    public struct ColliderComponent
    {
        public ColliderComponent()
        {
            ActiveCollisions = new HashSet<EntityReference>();
        }

        public float Radius;
        public Vector2 Offset;
        public HashSet<EntityReference> ActiveCollisions;
        public Action<Entity, Entity>? OnStart;
        public Action<Entity, Entity>? OnContinue;
        public Action<Entity, Entity>? OnExit;
    }
}

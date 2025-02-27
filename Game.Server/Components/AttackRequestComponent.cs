using Arch.Core;
using System.Numerics;

namespace Game.Server.Components
{
    public struct AttackRequestEventComponent
    {
        public EntityReference CastingEntity;
        public Vector2 MouseDirection;
    }
}

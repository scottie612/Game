using Arch.Core;
using System;
using System.Numerics;

namespace Game.Server.Components
{
    public struct OnAttackComponent
    {
        /// <summary>
        /// {EntityReference} is the Casting Entity
        /// {Vector2} is the Mouse Direction
        /// </summary>
        public Action<EntityReference, Vector2> OnAttack { get; set; }
    }
}

//using Arch.Buffer;
//using Arch.Core;
//using Arch.Core.Extensions;
//using Arch.System;
//using Game.EntityComponentSystem.Components;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Numerics;

//namespace Game.EntityComponentSystem.Systems
//{

//    public class DamageOnCollisionSystem : BaseSystem<World, float>
//    {
//        private ILogger<DamageOnCollisionSystem> _logger;

//        public DamageOnCollisionSystem(World world, ILogger<DamageOnCollisionSystem> logger) : base(world)
//        {
//            _logger = logger;
//        }

//        public override void Update(in float t)
//        {
//            var hitboxQuery = new QueryDescription().WithAll<HitboxComponent, PositionComponent>();
//            var damageQuery = new QueryDescription().WithAll<DamageOnCollisionComponent, PositionComponent>();

//            var buffer = new CommandBuffer();
//            World.Query(in damageQuery, (Entity entityWithDamage, ref DamageOnCollisionComponent damageComponent, ref PositionComponent damagePos) =>
//            {
//                var tempDamage = damageComponent;
//                var tempPosition = damagePos;

//                World.Query(in hitboxQuery, (Entity hitboxEntity, ref HitboxComponent hitbox, ref PositionComponent hitboxPos) =>
//                {
//                    // If it is the same entity return
//                    if (entityWithDamage == hitboxEntity) return;

//                    // If the entity was casted, dont hit the casting player.
//                    if (entityWithDamage.TryGet<CasterComponent>(out var caster))
//                    {
//                        if (caster.CastingEntity == hitboxEntity) return;
//                    }

//                    if (CheckCollision(tempPosition.Value, hitboxPos.Value, hitbox))
//                    {
//                        ApplyDamage(hitboxEntity, entityWithDamage, tempDamage);
//                        buffer.Add<DeleteEntityTag>(entityWithDamage);
//                    }


//                });
//            });
//            buffer.Playback(World);
//        }

//        private bool CheckCollision(Vector2 projectilePos, Vector2 targetPos, HitboxComponent targetHitbox)
//        {
//            float halfWidth = targetHitbox.Width / 2f;
//            float halfHeight = targetHitbox.Height / 2f;

//            float left = targetPos.X - halfWidth + targetHitbox.XOffset;
//            float right = targetPos.X + halfWidth + targetHitbox.XOffset;
//            float top = targetPos.Y + halfHeight + targetHitbox.YOffset;
//            float bottom = targetPos.Y - halfHeight + targetHitbox.YOffset;

//            return  projectilePos.X >= left && projectilePos.X <= right && projectilePos.Y >= bottom && projectilePos.Y <= top;

//        }

//        private void ApplyDamage(Entity target, Entity projectile, DamageOnCollisionComponent damageComponent)
//        {
//            if (target.Has<HealthComponent>())
//            {
//                World.TryGet<HealthComponent>(target, out var healthComponent);
//                int newHealth = Math.Max(0, healthComponent.CurrentValue - damageComponent.Value);

//                target.Set<HealthComponent>(new HealthComponent
//                {
//                    MaxValue = healthComponent.MaxValue,
//                    CurrentValue = newHealth
//                });

//                _logger.LogInformation($"Entity {target} took {damageComponent.Value} damage from projectile {projectile}. New health: {newHealth}");

//                if (newHealth <= 0)
//                {
//                    _logger.LogInformation($"Entity {target} has been defeated!");
//                    // Handle entity defeat (you might want to create a separate system for this)
//                }
//            }
//        }
//    }
//}

using Arch.Core;
using Arch.Core.Extensions;

namespace Game.Server.Components.Stats
{
    public struct HealthComponent
    {
        public HealthComponent(int maxValue)
        {
            MaxValue = maxValue;
            CurrentValue = maxValue;
        }

        public int MaxValue { get; private set; }
        public int CurrentValue { get; private set; }

        public void Heal(EntityReference entity, int amount)
        {
            CurrentValue += amount;
            entity.Entity.Add<HealthDirtyTag>();
            if (CurrentValue > MaxValue)
            {
                CurrentValue = MaxValue;
            }

        }

        public bool TakeDamage(EntityReference entity, int amount)
        {
            CurrentValue -= amount;
            entity.Entity.Add<HealthDirtyTag>();
            if (CurrentValue <= 0)
            {
                CurrentValue = 0;
                return true;
            }
            return false;
        }
    }
}

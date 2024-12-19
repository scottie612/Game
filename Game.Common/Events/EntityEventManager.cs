using System.Collections.Generic;

namespace Game.Events
{
    public static class EntityEventManager<T> where T : IEntityEvent
    {
        private static readonly Dictionary<int, List<IEntityEventListener<T>>> EventListeners = new Dictionary<int, List<IEntityEventListener<T>>>();
        private static readonly List<IEntityEventListener<T>> GlobalEventListeners = new List<IEntityEventListener<T>>();

        public static void Subscribe(int entityId, IEntityEventListener<T> listener)
        {
            if (!EventListeners.TryGetValue(entityId, out var listeners))
            {
                listeners = new List<IEntityEventListener<T>>();
                EventListeners[entityId] = listeners;
            }
            listeners.Add(listener);
        }

        public static void Subscribe(IEntityEventListener<T> listener) {

            if (!GlobalEventListeners.Contains(listener))
            {
                GlobalEventListeners.Add(listener);
            }
        }


        public static void Unsubscribe(int entityId, IEntityEventListener<T> listener)
        {
            if (EventListeners.TryGetValue(entityId, out var listeners))
            {
                listeners.Remove(listener);
                if (listeners.Count == 0)
                {
                    EventListeners.Remove(entityId);
                }
            }
        }
        public static void Unsubscribe(IEntityEventListener<T> listener)
        {
            if (GlobalEventListeners.Contains(listener))
            {
                GlobalEventListeners.Remove(listener);
            }
        }

        public static void RaiseEvent(T eventPacket)
        {
            if (EventListeners.TryGetValue(eventPacket.EntityID, out var listeners))
            {
                foreach (var listener in listeners)
                {
                    listener.OnEvent(eventPacket);
                }
            }

            foreach (var globalListener in GlobalEventListeners)
            {
                globalListener.OnEvent(eventPacket);
            }
        }
    }
}

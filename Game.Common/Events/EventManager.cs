using LiteNetLib;
using System.Collections.Generic;

namespace Game.Common.Events
{
    public static class EventManager<T> where T : IEvent
    {
        private static readonly List<IEventListener<T>> EventListeners = new List<IEventListener<T>>();

        public static void Subscribe(IEventListener<T> listener)
        {
            if (!EventListeners.Contains(listener))
            {
                EventListeners.Add(listener);
            }
        }


        public static void Unsubscribe(IEventListener<T> listener)
        {
            if (EventListeners.Contains(listener))
            {
                EventListeners.Remove(listener);
            }
        }

        public static void RaiseEvent(T eventPacket, NetPeer peer)
        {
            foreach (var globalListener in EventListeners)
            {
                globalListener.OnEvent(eventPacket, peer);
            }
        }
    }

    public interface IEventListener<T> where T : IEvent
    {
        void OnEvent(T eventPacket, NetPeer peer);
    }

    public interface IEvent
    {
    }
}

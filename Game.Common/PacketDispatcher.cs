using Game.Common.Enums;
using Game.Common.Packets.Interfaces;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Game.Common
{
    public class PacketDispatcher
    {
        public PacketDispatcher()
        {
            RegisterPackets();
        }

        #region RegisterPackets

        private Dictionary<PacketType, Func<IPacket>> _packetFactories = new Dictionary<PacketType, Func<IPacket>>();
        private void RegisterPackets()
        {

            Assembly assembly = Assembly.GetExecutingAssembly();

            // Find all types that inherit from IPacket
            var packetTypes = assembly.GetTypes().Where(t => t.IsValueType && typeof(IPacket).IsAssignableFrom(t));

            foreach (var type in packetTypes)
            {
                if (type != null)
                {
                    var packetInstance = (IPacket)Activator.CreateInstance(type);

                    // Cache a factory method to avoid repeated reflection calls
                    _packetFactories[packetInstance.PacketType] = () => (IPacket)Activator.CreateInstance(type);
                }
            }
        }

        public List<IPacket> Deserialize(PacketType type, NetDataReader reader)
        {
            List<IPacket> returnData = new List<IPacket>();
            if (_packetFactories.TryGetValue(type, out var factory))
            {
                var instance = factory();

                if (instance.IsBatched)
                {
                    var count = reader.GetByte();
                    for (int i = 0; i < count; i++)
                    {
                        instance = factory();
                        instance.Deserialize(reader);
                        returnData.Add(instance);
                    }
                }
                else
                {
                    instance.Deserialize(reader);
                    returnData.Add(instance);
                }
            }

            return returnData;
        }

        #endregion

        #region Handle Packets
        private Dictionary<Type, List<Action<NetPeer, IPacket>>> _handlers = new Dictionary<Type, List<Action<NetPeer, IPacket>>>();

        public void Subscribe<T>(Action<NetPeer, T> function) where T : IPacket
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var listeners))
            {
                listeners = new List<Action<NetPeer, IPacket>>();
                _handlers[type] = listeners;
            }
            listeners.Add((peer, packet) => function(peer, (T)packet));
        }

        public void Unsubscribe<T>(Action<NetPeer, T> function) where T : IPacket
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var listeners))
            {
                listeners.Remove((peer, packet) => function(peer, (T)packet));
                if (listeners.Count == 0)
                {
                    _handlers.Remove(type);
                }
            }
        }

        public void RaiseEvent<T>(NetPeer peer, T eventPacket) where T : IPacket
        {
            if (_handlers.TryGetValue(eventPacket.GetType(), out var listeners))
            {
                foreach (var listener in listeners)
                {
                    listener.Invoke(peer, eventPacket);
                }
            }
        }
        #endregion

        #region Send Packets
        private Dictionary<Type, Queue<IPacket>> _packetQueues = new Dictionary<Type, Queue<IPacket>>();

        public void Enqueue<T>(T packet) where T : IPacket
        {
            if (!_packetQueues.TryGetValue(packet.GetType(), out var queue))
            {
                queue = new Queue<IPacket>();
                _packetQueues[packet.GetType()] = queue;
            }
            queue.Enqueue(packet);
        }

        public void SendAllPackets(NetManager netManager, NetDataWriter writer)
        {
            foreach (var (type, queue) in _packetQueues)
            {
                if (queue.Count == 0)
                    continue;

                var firstPacket = queue.Peek();

                if (firstPacket.IsBatched)
                {
                    var batchSize = 30;
                    var batches = (int)Math.Ceiling((double)queue.Count / batchSize);

                    for (int i = 0; i < batches; i++)
                    {
                        writer.Put((byte)firstPacket.PacketType);

                        var count = Math.Min(queue.Count, 30);
                        writer.Put((byte)count);
                        for (int j = 0; j < count; j++)
                        {
                            var currentPacket = queue.Dequeue();
                            currentPacket.Serialize(writer);
                        }
                        netManager.SendToAll(writer, firstPacket.DeliveryMethod);
                        writer.Reset();
                    }
                }
                else
                {
                    foreach (IPacket packet in queue)
                    {
                        writer.Put((byte)packet.PacketType);
                        packet.Serialize(writer);
                        if (packet.NetPeer == null)
                        {
                            netManager.SendToAll(writer, packet.DeliveryMethod);
                        }
                        else
                        {
                            packet.NetPeer.Send(writer, packet.DeliveryMethod);
                        }
                        writer.Reset();
                    }
                }

                queue.Clear();
            }
        }

        #endregion
    }
}

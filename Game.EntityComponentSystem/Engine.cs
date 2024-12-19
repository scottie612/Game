using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Game.Configuration;
using Game.EntityComponentSystem.Systems;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Hosting;
using Schedulers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Game.EntityComponentSystem
{
    public class Engine : BackgroundService, INetEventListener
    {

        private World _world;
        private NetManager _netManager;
        private Group<float> _systems = new Group<float>("Game Systems");
        internal static JobScheduler _jobScheduler = new JobScheduler(new JobScheduler.Config());
        public Dictionary<Packet, Action<NetDataReader, NetPeer>> _packetHandlers = new Dictionary<Packet, Action<NetDataReader, NetPeer>>();
        public Engine()
        {
            _world = World.Create();
            _netManager = new NetManager(this);
        }

        private void RegisterSystems()
        {
            _systems.Add(new SpawningSystem(_world, _netManager));
            _systems.Add(new MovementSystem(_world, _netManager));
        }

        private void RegisterPacketHandlers()
        {
            _packetHandlers.Add(Packet.MovementRequest, _systems.Get<MovementSystem>().HandleMovementRequest);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cappedDeltaTime = 16.67f; // Capped delta time in milliseconds

            _world = World.Create();

            _netManager.Start(7777);

            RegisterSystems();
            RegisterPacketHandlers();

            _systems.Initialize();


            var deltaTime = cappedDeltaTime;
            while (!stoppingToken.IsCancellationRequested)
            {
                var startTime = DateTime.UtcNow;

                _netManager.PollEvents();
                _systems.BeforeUpdate(in deltaTime);
                _systems.Update(in deltaTime);
                _systems.AfterUpdate(in deltaTime);

                var elapsedTime = (float)(DateTime.UtcNow - startTime).TotalMilliseconds;

                if (elapsedTime < cappedDeltaTime)
                {
                    var remainingTime = cappedDeltaTime - elapsedTime;
                    await Task.Delay(TimeSpan.FromMilliseconds(remainingTime), stoppingToken);
                    deltaTime = cappedDeltaTime;
                }
                else
                {
                    deltaTime = elapsedTime;
                }

                if (deltaTime != cappedDeltaTime)
                {
                    System.Console.WriteLine($"DeltaTime: {deltaTime.ToString()}");
                }
            }

            _systems.Dispose();
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            System.Console.WriteLine($"Incoming Connection Request from: {request.RemoteEndPoint}");
            request.Accept();
        }

        public void OnPeerConnected(NetPeer peer)
        {
            System.Console.WriteLine($"Client Connected! Ping: {peer.Ping}");
            EntityFactory.CreatePlayer(_world, peer);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            var query = new QueryDescription().WithAll<NetworkConnectionComponent>();
            _world.Query(query, (Entity entity, ref NetworkConnectionComponent pc) =>
            {
                if (pc.Peer.Id == peer.Id)
                {
                    System.Console.WriteLine($"Client Disconnected! ID: {entity.Id}");
                    entity.Add(new DeleteEntityTag { });
                }
            });
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            var packetType = (Packet)reader.GetByte();
            if (_packetHandlers.TryGetValue(packetType, out var handler))
            {
                handler?.Invoke(reader, peer);
            }
            else
            {
                System.Console.WriteLine("No Packet Handler found for {0}", packetType);
            }
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {

        }
        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {

        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {

        }


    }
}

using Arch.Buffer;
using Arch.Core;
using Game.Common;
using Game.Common.Encryption;
using Game.Common.Enums;
using Game.Common.Packets.Interfaces;
using Game.Server.Components;
using Game.Server.Entities;
using Game.Server.Options;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ServerModels;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Game.Server.Systems
{
    public class NetworkingSystem : SystemBase, INetEventListener
    {
        private NetManager _netManager;
        private NetDataWriter _writer;

        private readonly ServerOptions _serverOptions;
        private readonly EncryptionOptions _encryptionOptions;
        private readonly ILogger<NetworkingSystem> _logger;

        private QueryDescription _networkConnectionQuery = new QueryDescription().WithAll<NetworkConnectionComponent>();

        public NetworkingSystem(GameWorld world, PacketDispatcher packetDispatcher, IOptions<ServerOptions> serverOptions, IOptions<EncryptionOptions> encryptionOptions, ILogger<NetworkingSystem> logger) : base(world, packetDispatcher)
        {
            _serverOptions = serverOptions.Value;
            _encryptionOptions = encryptionOptions.Value;
            _logger = logger;
            _writer = new NetDataWriter();
            _netManager = new NetManager(this)
            {
                AutoRecycle = true,
                UnconnectedMessagesEnabled = false,
                DisconnectTimeout = _serverOptions.DisconnectTimeout
            };
        }

        public override void Initialize()
        {
            _logger.LogTrace($"Server starting on port: {_serverOptions.Port}");
            _netManager.Start(_serverOptions.Port);
        }

        public override void Update(float deltaTime)
        {
            _netManager.PollEvents();
            PacketDispatcher.SendAllPackets(_netManager, _writer);
        }

        public override void Shutdown()
        {
            _netManager.Stop();
        }


        public void OnConnectionRequest(ConnectionRequest request)
        {

            _logger.LogTrace($"Incoming Connection Request from: {request.RemoteEndPoint}");

            if (_netManager.ConnectedPeersCount >= _serverOptions.MaxConnections)
            {
                request.Reject();
                _logger.LogTrace($"Request from: {request.RemoteEndPoint} rejected due to connection limit.");
            }

           

            //TODO: Add Null Checks and Validations. Move to separate method/class? Maybe in seperate method, return bool and error/success message.
            Task.Run(async () =>
            {
                var clientPublicKey = request.Data.GetString();
                var encryptedAesKey = request.Data.GetString();
                var encryptedAuthData = request.Data.GetString();
                var signature = request.Data.GetString();

                //Decrypt AES key with private key
                var aesKey = EncryptionHelper.Decrypt(_encryptionOptions.PrivateKey, encryptedAesKey);

                //Decrypt AuthData with AES key
                var authDataJson = EncryptionHelper.DecryptAes(aesKey, encryptedAuthData);
                var authData = JsonConvert.DeserializeObject<AuthData>(authDataJson);


                //Validate Signature using User's Public Key
                var isValidSignature = EncryptionHelper.ValidateSignature(clientPublicKey, authDataJson, signature);

                //Validate Protocol Version

                //Validate Nonce

                //Validate Timestamp

                //Verify the Session Ticket
                var result = await PlayFabServerAPI.AuthenticateSessionTicketAsync(new AuthenticateSessionTicketRequest()
                {
                    SessionTicket = authData.SessionTicket,
                });

                if (result != null)
                {
                    if (result.Result != null && result.Result.IsSessionTicketExpired == false)
                    {

                        var connectedPeer = request.Accept();

                        var player = PlayerFactory.CreatePlayer(World.World, connectedPeer, result.Result.UserInfo.Username);

                        _logger.LogTrace($"Player connection request Accepted! Player: {result.Result.UserInfo.Username}, IP: {request.RemoteEndPoint}");
                    }
                    else
                    {
                        _logger.LogTrace($"Player connection request rejected! IP: {request.RemoteEndPoint}");
                        request.Reject();
                    }
                }
            });
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            PacketType type = (PacketType)reader.GetByte();

            List<IPacket> packets = PacketDispatcher.Deserialize(type, reader);
            if (packets.Count != 0)
            {
                foreach (IPacket packet in packets)
                {
                    PacketDispatcher.RaiseEvent(peer, packet);
                }
            }
            else
            {
                _logger.LogError("Packet Type not found or failed to deserialize packet");
            }
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
           
        }

        public void OnPeerConnected(NetPeer peer)
        {
            _logger.LogTrace($"Client Connected! IP: {peer.Address}; Ping:{peer.Ping}");
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            var buffer = new CommandBuffer();
            World.World.Query(_networkConnectionQuery, (Entity entity, ref NetworkConnectionComponent pc) =>
            {
                if (pc.Peer.Id == peer.Id)
                {
                    //TODO.. Save Player

                    //Remove player from the world
                    buffer.Add(entity, new DeleteEntityTag { });
                }
            });
            buffer.Playback(World.World);
            buffer.Dispose();
            _logger.LogTrace($"Client Disconnected! IP: {peer.Address}; Ping:{peer.Ping}");
        }
    }
}

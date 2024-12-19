using Game.Configuration;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Game.Console.Helpers
{
    public class BatchPacketProcessor
    {
        private int _totalUpdateCount;
        private Packet _packetType;
        private DeliveryMethod _deliveryMethod;
        private int _maxUpdatesPerPacket = 30;
        private int _processedEntities = 0;
        private int _entitiesInCurrentBatch = 0;
        private NetDataWriter _writer;
        private NetManager _netManager;
        private int batchCount = 1;
        public BatchPacketProcessor(Packet type, DeliveryMethod deliveryMethod, NetDataWriter netDataWriter, NetManager netManager, int maxUpdatesPerPacket = 30)
        {
            _netManager = netManager;
            _writer = netDataWriter;
            _packetType = type;
            _maxUpdatesPerPacket = maxUpdatesPerPacket;
            _deliveryMethod = deliveryMethod;

        }

        public void Reset(int totalUpdateCount)
        {
            _totalUpdateCount = totalUpdateCount;
            _processedEntities = 0;
            _entitiesInCurrentBatch = 0;
            batchCount = 0;
            _writer.Reset();
            _writer.Put((byte)_packetType);
            _writer.Put((ushort)0); // Placeholder for batch size
        }

        public void ProcessEntity(INetSerializable entityToUpdate)
        {
            entityToUpdate.Serialize(_writer);
            _entitiesInCurrentBatch++;
            _processedEntities++;
            
            if (_entitiesInCurrentBatch == _maxUpdatesPerPacket || _processedEntities == _totalUpdateCount)
            {
                // Write the actual batch size
                _writer.SetPosition(1);
                _writer.Put((ushort)_entitiesInCurrentBatch);
                _writer.SetPosition(_writer.Capacity);

                _netManager.SendToAll(_writer, _deliveryMethod);

                if (_processedEntities < _totalUpdateCount)
                {
                    // Prepare for the next batch
                    _writer.Reset();
                    _writer.Put((byte)_packetType);
                    _writer.Put((ushort)0); // Placeholder for next batch size
                    _entitiesInCurrentBatch = 0;
                    batchCount++;
                }
            }
        }
    }
}

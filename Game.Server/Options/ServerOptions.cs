namespace Game.Server.Options
{
    public class ServerOptions
    {
        public int MaxConnections { get; set; }
        public int Port { get; set; }
        public int DisconnectTimeout { get; set; }
        public int TickRate { get; set; }
        public MaxWorldSize MaxWorldSize { get; set; }
    }

    public class MaxWorldSize
    {
        public int MaxX { get; set; }
        public int MinX { get; set; }
        public int MaxY { get; set; }
        public int MinY { get; set; }
    }
}

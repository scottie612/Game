namespace Game.Server.Options
{
    public class ServerOptions
    {
        public int MaxConnections { get; set; }
        public int Port { get; set; }
        public int DisconnectTimeout { get; set; }
        public int TickRate { get; set; }
    }
}

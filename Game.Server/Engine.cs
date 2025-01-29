using Game.Server.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Game.Server
{
    public class Engine : BackgroundService
    {

        private readonly ILogger<Engine> _logger;
        private readonly GameWorld _gameWorld;
        private readonly ServerOptions _serverOptions;

        public Engine(GameWorld gameWorld, IOptions<ServerOptions> serverOptions, ILogger<Engine> logger)
        {
            _gameWorld = gameWorld;
            _serverOptions = serverOptions.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //convert tick rate to seconds (60hz -> 16.66666ms)
            double updateInterval = (1000 / (double)_serverOptions.TickRate);

            _gameWorld.OnInitialize?.Invoke();

            var stopwatch = new Stopwatch();

            double executionTime = 0d;
            double totalFrameTime = 0d;
            double timeWaited = 0d;

            while (!stoppingToken.IsCancellationRequested)
            {
                stopwatch.Restart();
                timeWaited = 0d;
                var deltaTime = (float)(totalFrameTime / 1000);
                _gameWorld.OnUpdate?.Invoke(deltaTime);

                executionTime = stopwatch.Elapsed.TotalMilliseconds;

                if (executionTime < updateInterval)
                {
                    while (stopwatch.Elapsed.TotalMilliseconds < updateInterval)
                    {
                        Thread.SpinWait(1);
                    }
                    timeWaited = stopwatch.Elapsed.TotalMilliseconds - executionTime;
                }

                if (executionTime > updateInterval * 1.05f)
                {
                    _logger.LogWarning($"Lagging, Execution time: {executionTime:F3}ms");
                }

                totalFrameTime = stopwatch.Elapsed.TotalMilliseconds;
                //_logger.LogTrace($"ExecutionTime: {executionTime}, TimeWaited: {timeWaited}, TotalFrameTime: {totalFrameTime}, DeltaTime: {deltaTime}");
            }

            _gameWorld.OnShutdown?.Invoke();
        }
    }
}

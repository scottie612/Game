using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Game.Server
{
    public class Engine: BackgroundService
    {

        private readonly ILogger<Engine> _logger;
        private readonly GameWorld _gameWorld;
        public Engine(GameWorld gameWorld, ILogger<Engine> logger) 
        { 
            _gameWorld = gameWorld;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            _gameWorld.OnInitialize?.Invoke();

            var cappedDeltaTime = 10f;

            var deltaTime = cappedDeltaTime;
            while (!stoppingToken.IsCancellationRequested)
            {
                var startTime = DateTime.UtcNow;

                _gameWorld.OnUpdate?.Invoke(deltaTime);

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
                    _logger.LogTrace($"DeltaTime: {deltaTime.ToString()}");
                }
            }

            _gameWorld.OnShutdown?.Invoke();

        }
    }
}

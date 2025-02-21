using Game.Common;
using Game.Common.Extentions;
using Game.Server;
using Game.Server.Components;
using Game.Server.Components.Stats;
using Game.Server.Systems;
using Microsoft.Extensions.Logging;
using System.Numerics;

namespace Game.Tests
{
    [TestFixture]
    public class MovementSystemTest
    {
        private MovementSystem _movementSystem;
        private GameWorld _world;
        private PacketDispatcher _packetDispatcher;
        private ILogger<MovementSystem> _logger;

        [SetUp]
        public void SetUp()
        {
            _world = new GameWorld();
            _packetDispatcher = new PacketDispatcher();
            _logger = new Logger<MovementSystem>(new LoggerFactory());
            _movementSystem = new MovementSystem(_world, _packetDispatcher, _logger);
        }

        [TearDown]
        public void TearDown()
        {
            _world.World.Dispose();
            _movementSystem.Shutdown();
        }

        public static IEnumerable<TestCaseData> MovementTestCases()
        {

            yield return new TestCaseData(
                new PositionComponent { Value = new Vector2(0, 0) },
                new VelocityComponent { Value = new Vector2(1, 0).NormalizeSafe() },
                new MovementSpeedComponent { Value = 10f },
                1000f,
                new Vector2(10, 0)
            ).SetName("Move Right");

            yield return new TestCaseData(
                new PositionComponent { Value = new Vector2(5, 5) },
                new VelocityComponent { Value = new Vector2(0, 1).NormalizeSafe() },
                new MovementSpeedComponent { Value = 5f },
                2000f,
                new Vector2(5, 15)
            ).SetName("Move Up");

            yield return new TestCaseData(
                new PositionComponent { Value = new Vector2(-1, -1) },
                new VelocityComponent { Value = new Vector2(-1, -1).NormalizeSafe() },
                new MovementSpeedComponent { Value = 2f },
                500f,
                new Vector2(-1.7071068f, -1.7071068f)
            ).SetName("Move Diagonal");

            yield return new TestCaseData(
                new PositionComponent { Value = new Vector2(-1, -1) },
                new VelocityComponent { Value = new Vector2(0, 0).NormalizeSafe() },
                new MovementSpeedComponent { Value = 2f },
                500f,
                new Vector2(-1f, -1f)
            ).SetName("Not Moving");
        }

        [Test]
        [TestCaseSource(nameof(MovementTestCases))]
        public void Update_MovesEntityCorrectly(PositionComponent initialPosition, VelocityComponent direction, MovementSpeedComponent speed, float deltaTime, Vector2 expectedPosition)
        {
            // Arrange
            var entity = _world.World.Create();
            _world.World.Add(entity, initialPosition);
            _world.World.Add(entity, direction);
            _world.World.Add(entity, speed);

            // Act
            _movementSystem.Update(deltaTime);

            // Assert
            var finalPosition = _world.World.Get<PositionComponent>(entity);
            Assert.That(finalPosition.Value, Is.EqualTo(expectedPosition));
        }

    }
}

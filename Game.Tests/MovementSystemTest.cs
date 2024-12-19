using Arch.Core;
using Game.EntityComponentSystem;
using Game.EntityComponentSystem.Components.Tags;
using Game.EntityComponentSystem.Systems;
using Game.Extentions;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Internal;
using System.Numerics;

namespace Game.Tests
{
    [TestFixture]
    public class MovementSystemTest
    {
        private World _world;
        private MovementSystem _movementSystem;
        private NetManager _netManager;

        [SetUp]
        public void SetUp()
        {
            _world = World.Create();
            _netManager = new NetManager(new EventBasedNetListener());
            _movementSystem = new MovementSystem(_world, _netManager);
        }

        [TearDown]
        public void TearDown()
        {
            _world.Dispose();
            _movementSystem.Dispose();
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
            var entity = _world.Create();
            _world.Add(entity, initialPosition);
            _world.Add(entity, direction);
            _world.Add(entity, speed);

            // Act
            _movementSystem.Update(deltaTime);

            // Assert
            var finalPosition = _world.Get<PositionComponent>(entity);
            Assert.That(finalPosition.Value, Is.EqualTo(expectedPosition));
        }

    }
}

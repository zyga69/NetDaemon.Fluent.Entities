using NetDaemon.Fluent.Entities.Extensions;
using NetDaemon.Fluent.Entities.Tests.Helpers;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NSubstitute;

namespace NetDaemon.Fluent.Entities.Tests.Extensions
{
    public class EntityExtensionsTests
    {
        private readonly IHaContext _haContext;

        public EntityExtensionsTests()
        {
            _haContext = Substitute.For<IHaContext>();
        }

        private Entity CreateEntity(string? state)
        {
            string entityId = $"binary_sensor.{StringHelpers.Random()}";
            _haContext.GetState(entityId).Returns(new EntityState { EntityId = entityId, State = state });
            return new Entity(_haContext, entityId);
        }

        [Fact]
        public void GetBinarySensorState_On_ReturnsTrue()
        {
            var entity = CreateEntity("on");

            var result = entity.GetBinarySensorState();

            Assert.True(result);
        }

        [Fact]
        public void GetBinarySensorState_Off_ReturnsFalse()
        {
            var entity = CreateEntity("off");

            var result = entity.GetBinarySensorState();

            Assert.False(result);
        }

        [Fact]
        public void GetBinarySensorState_NeitherOnNorOff_ReturnsNull()
        {
            var entity = CreateEntity("unavailable");

            var result = entity.GetBinarySensorState();

            Assert.Null(result);
        }
    }
}

using System;
using System.Reflection;
using System.Text.Json;
using Confluent.Kafka;
using FluentAssertions;
using Publo.Kafka.Producer;
using Xunit;

namespace Publo.Kafka.UnitTests.Producer;

public class KafkaProducerTests
{
    [Fact]
    public void JsonValueSerializer_ShouldSerializeMessageAsUtf8Json()
    {
        // Arrange
        var serializer = CreateSerializer<TestMessage>();
        var message = new TestMessage(42, "test");

        // Act
        var bytes = serializer.Serialize(message, SerializationContext.Empty);

        // Assert
        var deserialized = JsonSerializer.Deserialize<TestMessage>(bytes);
        deserialized.Should().Be(message);
    }

    private static ISerializer<T> CreateSerializer<T>()
    {
        var serializerType = typeof(KafkaProducer).GetNestedType("JsonValueSerializer`1", BindingFlags.NonPublic);

        serializerType.Should().NotBeNull();

        return (ISerializer<T>)Activator.CreateInstance(serializerType!.MakeGenericType(typeof(T)))!;
    }

    private sealed record TestMessage(int Value, string Name);
}

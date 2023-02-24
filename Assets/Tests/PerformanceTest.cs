using System;
using NUnit.Framework;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.PerformanceTesting;
using Unity.Properties;
using Unity.Serialization.Binary;

public class PerformanceTest
{
    [Test]
    public unsafe void SimpleSerializationTest()
    {
        var data = new TestData();

        using var stream = new UnsafeAppendBuffer(16, 8, Allocator.Temp);
        BinarySerialization.ToBinary(&stream, data);

        var reader = stream.AsReader();
        var deserialized = BinarySerialization.FromBinary<TestData>(&reader);

        Assert.IsTrue(data.Equals(deserialized));
    }

    [Test]
    public void SerializeAndDeserializeTest()
    {
        var data = new TestData();

        using var serialized = data.Serialize(Allocator.Temp);
        Debug.Log($"Serialized size: {serialized.Length} bytes");

        var deserialized = TestData.Deserialize(serialized);
        Debug.Log($"Deserialized: {deserialized}");

        Assert.IsTrue(data.Equals(deserialized));
    }

    [Test, Performance]
    public void UnitySerializeTest()
    {
        var data = new TestData();

        Measure.Method(() =>
            {
                var serialized = data.Serialize(Allocator.Temp);
                serialized.Dispose();
            })
            .WarmupCount(1)
            .MeasurementCount(20)
            .IterationsPerMeasurement(20)

            .Run();
        Debug.Log("Finished");
    }
}

[Serializable]
[GeneratePropertyBag]
public class TestData : IEquatable<TestData>
{
    public enum Mode
    {
        A, B, C
    }

    public string text = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";
    public int integer = 10;
    public float floating = 1.5f;
    public Vector3 vector = new(1, 2, 3);
    public Color color = new(1, 0, 0, 1);
    public Mode mode = Mode.A;
    public byte[] bytes = new byte[1000];

    public bool Equals(TestData other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return text == other.text
            && integer == other.integer
            && floating == other.floating
            && vector == other.vector
            && color == other.color
            && mode == other.mode
            && bytes.Length == other.bytes.Length;
    }

    public unsafe NativeArray<byte> Serialize(Allocator allocator)
    {
        using var stream = new UnsafeAppendBuffer(16, 8, Allocator.Temp);
        BinarySerialization.ToBinary(&stream, this);

        var result = new NativeArray<byte>(stream.Length, allocator);
        UnsafeUtility.MemCpy(result.GetUnsafePtr(), stream.Ptr, stream.Length);
        return result;
    }

    public static unsafe TestData Deserialize(NativeArray<byte> data)
    {
        using var stream = new UnsafeAppendBuffer(8, 8, Allocator.Temp);
        stream.Add(data.GetUnsafePtr(), data.Length);
        var reader = stream.AsReader();
        return BinarySerialization.FromBinary<TestData>(&reader);
    }

    public override string ToString()
    {
        return $"TestData {{ text: {text}, integer: {integer}, floating: {floating}, vector: {vector}, color: {color}, mode: {mode}, bytes: {bytes.Length} bytes }}";
    }
}
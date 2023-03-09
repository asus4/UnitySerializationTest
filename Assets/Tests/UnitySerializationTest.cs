using System.IO;
using NUnit.Framework;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.PerformanceTesting;
using Unity.Serialization.Binary;

public class UnitySerializationTest
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
        var deserialized = TestData.Deserialize(serialized);
        Assert.IsTrue(data.Equals(deserialized));
    }

    [Test]
    public void SaveToFileTest()
    {
        var data = new TestData();
        using var serialized = data.Serialize(Allocator.Temp);
        var path = Path.Combine(Application.temporaryCachePath, "test.bin");
        File.WriteAllBytes(path, serialized.ToArray());
        Assert.IsTrue(File.Exists(path));

        var bytes = File.ReadAllBytes(path);
        var deserialized = TestData.Deserialize(new NativeArray<byte>(bytes, Allocator.Temp));
        Assert.IsTrue(data.Equals(deserialized));
    }

    [Test, Performance]
    public void SerializeTest()
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

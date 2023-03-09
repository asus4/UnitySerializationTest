using System;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Properties;
using Unity.Serialization.Binary;

[Serializable]
[GeneratePropertyBag]
public partial class TestData : IEquatable<TestData>
{
    public enum Mode
    {
        A, B, C
    }

    public string text = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";
    public int integer = 10;
    public float floating = 1.5f;
    public Vector3 vector = new(1, 2, 3);
    public Quaternion quaternion = new(1, 0, 0, 1);
    public Color color = new(1, 0, 0, 1);
    public Mode mode = Mode.A;
    public byte[] bytes = new byte[1000];
    public Pose pose = new(new Vector3(1, 2, 3), new Quaternion(1, 0, 0, 1));
    public float4x4 floatMatrix = new(new float4(1, 0, 0, 0), new float4(0, 1, 0, 0), new float4(0, 0, 1, 0), new float4(0, 0, 0, 1));
    public Matrix4x4 matrix = new(new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
    public InnerClass innerClass = new();

    private static readonly UnsafeAppendBuffer s_Stream = new(16, 8, Allocator.Persistent);

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
            && bytes.Length == other.bytes.Length
            && pose == other.pose
            && floatMatrix.Equals(other.floatMatrix)
            && matrix == other.matrix
            && innerClass.Equals(other.innerClass);
    }

    public unsafe NativeArray<byte> Serialize(Allocator allocator)
    {
        fixed (UnsafeAppendBuffer* stream = &s_Stream)
        {
            BinarySerialization.ToBinary(stream, this);
            var result = new NativeArray<byte>(stream->Length, allocator);
            UnsafeUtility.MemCpy(result.GetUnsafePtr(), stream, stream->Length);
            return result;
        }
    }

    public static unsafe TestData Deserialize(NativeArray<byte> data)
    {
        s_Stream.Reset();
        fixed (UnsafeAppendBuffer* stream = &s_Stream)
        {
            stream->Add(data.GetUnsafePtr(), data.Length);
            var reader = stream->AsReader();
            return BinarySerialization.FromBinary<TestData>(&reader);
        }
    }

    public override string ToString()
    {
        return $"TestData {{ text: {text}, integer: {integer}, floating: {floating}, vector: {vector}, color: {color}, mode: {mode}, bytes: {bytes.Length} bytes }}";
    }
}

public partial class TestData
{
    [Serializable]
    [GeneratePropertyBag]
    public class InnerClass: IEquatable<InnerClass>
    {
        public string text = "inner class text";
        public bool Equals(InnerClass other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return text == other.text;
        }
    }
}

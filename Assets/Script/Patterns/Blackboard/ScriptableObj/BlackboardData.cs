using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Blackboard Data", menuName = "Blackboard/Blackboard Data")]
public class BlackboardData : ScriptableObject {
    public List<BlackboardEntryData> entries = new();

    public void SetValuesOnBlackboard(Blackboard blackboard) {
        foreach (var entry in entries) { entry.SetValuesOnBlackboard(blackboard); }
    }
}

[Serializable]
public class BlackboardEntryData : ISerializationCallbackReceiver {
    public string keyName;
    public AnyValue.ValueType valueType;
    public AnyValue value;

    public void SetValuesOnBlackboard(Blackboard blackboard) {
        var key = blackboard.GetOrRegister(keyName);
        setValueDispatchTable[value.type](blackboard, key, value);
    }

    private static Dictionary<AnyValue.ValueType, Action<Blackboard, BlackboardKey, AnyValue>> setValueDispatchTable
        = new() {
            { AnyValue.ValueType.Bool, (blackboard, key, anyValue) => blackboard.SetValue<bool>(key, anyValue) },
            { AnyValue.ValueType.Int, (blackboard, key, anyValue) => blackboard.SetValue<int>(key, anyValue) },
            { AnyValue.ValueType.Float, (blackboard, key, anyValue) => blackboard.SetValue<float>(key, anyValue) }, {
                AnyValue.ValueType.String, (blackboard, key, anyValue) => blackboard.SetValue<string>(key, anyValue)
            }, {
                AnyValue.ValueType.Vector2,
                (blackboard, key, anyValue) => blackboard.SetValue<Vector2>(key, anyValue)
            }, {
                AnyValue.ValueType.Vector2Int,
                (blackboard, key, anyValue) => blackboard.SetValue<Vector2Int>(key, anyValue)
            }, {
                AnyValue.ValueType.Vector3,
                (blackboard, key, anyValue) => blackboard.SetValue<Vector3>(key, anyValue)
            }, {
                AnyValue.ValueType.Vector3Int,
                (blackboard, key, anyValue) => blackboard.SetValue<Vector3Int>(key, anyValue)
            },
        };

    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize() => value.type = valueType;
}

[Serializable]
public struct AnyValue {
    public enum ValueType {
        Int,
        Float,
        Bool,
        String,
        Vector2,
        Vector2Int,
        Vector3,
        Vector3Int,
        Sprite,
    }

    [FormerlySerializedAs("Type")] public ValueType type;

    //Storage for different value types
    [FormerlySerializedAs("BoolValue")] public bool boolValue;
    [FormerlySerializedAs("IntValue")] public int intValue;
    [FormerlySerializedAs("FloatValue")] public float floatValue;
    [FormerlySerializedAs("StringValue")] public string stringValue;
    [FormerlySerializedAs("Vector2Value")] public Vector2 vector2Value;

    [FormerlySerializedAs("Vector2IntValue")]
    public Vector2Int vector2IntValue;

    [FormerlySerializedAs("Vector3Value")] public Vector3 vector3Value;

    [FormerlySerializedAs("Vector3IntValue")]
    public Vector3Int vector3IntValue;

    public Sprite spriteValue;


    //Method to convert primitive data types to generic types with safety and without boxing
    T AsBool<T>(bool value) => typeof(T) == typeof(bool) && value is T correctType ? correctType : default;
    T AsInt<T>(int value) => typeof(T) == typeof(int) && value is T correctType ? correctType : default;
    T AsFloat<T>(float value) => typeof(T) == typeof(float) && value is T correctType ? correctType : default;
    T AsVector2<T>(Vector2 value) => typeof(T) == typeof(Vector2) && value is T correctType ? correctType : default;

    T AsVector2Int<T>(Vector2Int value) =>
        typeof(T) == typeof(Vector2Int) && value is T correctType ? correctType : default;

    T AsVector3<T>(Vector3 value) => typeof(T) == typeof(Vector3) && value is T correctType ? correctType : default;

    T AsVector3Int<T>(Vector3Int value) =>
        typeof(T) == typeof(Vector3Int) && value is T correctType ? correctType : default;

    T AsSprite<T>(Sprite value) => typeof(T) == typeof(Sprite) && value is T correctType ? correctType : default;

    //Implicit conversion operators to convert AnyValue to different types
    public static implicit operator bool(AnyValue value) => value.ConvertValue<bool>();
    public static implicit operator string(AnyValue value) => value.ConvertValue<string>();
    public static implicit operator int(AnyValue value) => value.ConvertValue<int>();
    public static implicit operator float(AnyValue value) => value.ConvertValue<float>();
    public static implicit operator Vector2(AnyValue value) => value.ConvertValue<Vector2>();
    public static implicit operator Vector2Int(AnyValue value) => value.ConvertValue<Vector2Int>();
    public static implicit operator Vector3(AnyValue value) => value.ConvertValue<Vector3>();
    public static implicit operator Vector3Int(AnyValue value) => value.ConvertValue<Vector3Int>();
    public static implicit operator Sprite(AnyValue value) => value.ConvertValue<Sprite>();

    T ConvertValue<T>() {
        return type switch {
            ValueType.Int => AsInt<T>(intValue),
            ValueType.Float => AsFloat<T>(floatValue),
            ValueType.Bool => AsBool<T>(boolValue),
            ValueType.String => (T)(object)stringValue,
            ValueType.Vector2 => AsVector2<T>(vector2Value),
            ValueType.Vector2Int => AsVector2Int<T>(vector2IntValue),
            ValueType.Vector3 => AsVector3<T>(vector3Value),
            ValueType.Vector3Int => AsVector3Int<T>(vector3IntValue),
            ValueType.Sprite => AsSprite<T>(spriteValue),
            _ => throw new NotSupportedException($"Unsupported value type: {typeof(T)}")
        };
    }
}
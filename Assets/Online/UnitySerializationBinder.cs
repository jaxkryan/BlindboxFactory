using Newtonsoft.Json.Serialization;
using System;
using UnityEngine;

public class UnitySerializationBinder : ISerializationBinder
{
    public Type BindToType(string assemblyName, string typeName)
    {
        try
        {
            // Handle Unity's default assembly
            if (assemblyName == "Assembly-CSharp")
            {
                return Type.GetType($"{typeName}, Assembly-CSharp");
            }
            // Handle mscorlib or other system assemblies
            if (assemblyName == "mscorlib")
            {
                return Type.GetType(typeName);
            }
            // Fallback for other assemblies
            return Type.GetType($"{typeName}, {assemblyName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to bind type {typeName}, {assemblyName}: {e.Message}");
            return null; // Return null to let deserializer handle missing types
        }
    }

    public void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
        assemblyName = serializedType.Assembly.GetName().Name;
        typeName = serializedType.FullName;
    }
}
using System;
using UnityEngine;

public static class GameObjectExtension {
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component {
        if (go == null) return null;

        if (go.GetComponent<T>() == null) go.AddComponent<T>();
        return go.GetComponent<T>();
    }
}
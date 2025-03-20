using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PersistentSingleton<T> : MonoBehaviour where T : Component {
    public bool AutoUnparentOnLoad = true;
    protected static T instance;

    public static bool HasInstance => instance != null;
    public static T TryGetInstance => HasInstance ? instance : null;

    public static T Instance {
        get {
            if (instance == null) {
                instance = FindAnyObjectByType<T>();
                if (instance == null && Application.isPlaying) {
                    var go = new GameObject(typeof(T).Name + " Auto-Generated");
                    instance = go.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    protected virtual void Awake() {
        InitializeSingleton();
    }

    protected virtual void InitializeSingleton() {
        if (Application.isPlaying) return;

        if (AutoUnparentOnLoad) transform.SetParent(null);

        if (instance == null) {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }
}

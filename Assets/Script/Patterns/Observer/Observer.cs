using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public class Observer<T> {
    [SerializeField] T value;
    [SerializeField] UnityEvent<T> onValueChanged;

    public T Value {
        get { return value; }
        set { Set(value); }
    }

    public static implicit operator T(Observer<T> o) => o.value;

    public Observer(T value, UnityAction<T> callback = null) {
        this.value = value;
        onValueChanged = new UnityEvent<T>();
        if (callback != null) onValueChanged.AddListener(callback);
    }

    public void Set(T value) {
        if (Equals(value, this.value)) return;
        this.value = value;
        Invoke();
    }

    public void Invoke() { onValueChanged.Invoke(value); }

    public void AddListener(UnityAction<T> callback) {
        if (callback == null) return;
        if (onValueChanged == null) onValueChanged = new UnityEvent<T>();

        #if UNITY_EDITOR
        UnityEditor.Events.UnityEventTools.AddPersistentListener(onValueChanged, callback);
        #else
        onValueChanged.AddListener(callback);
        #endif
    }

    public void RemoveListener(UnityAction<T> callback) {
        if (callback == null) return;
        if (onValueChanged == null) return;

        #if UNITY_EDITOR
        UnityEditor.Events.UnityEventTools.RemovePersistentListener(onValueChanged, callback);
        #else
        onValueChanged.RemoveListener(callback);
        #endif
    }

    public void RemoveAllListeners() {
        if (onValueChanged == null) return;
        
        #if UNITY_EDITOR
        FieldInfo fieldInfo = typeof(UnityEventBase).GetField("m_PersistentCalls", BindingFlags.Instance | BindingFlags.NonPublic);
        object value = fieldInfo.GetValue(onValueChanged);
        value.GetType().GetMethod("Clear").Invoke(value, null);
        #else
        onValueChanged.RemoveAllListeners();
        #endif
    }

    public void Dispose() {
        RemoveAllListeners();
        onValueChanged = null;
        value = default(T);
    }
}
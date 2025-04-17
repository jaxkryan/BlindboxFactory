using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : PersistentSingleton<UnityMainThreadDispatcher> {
    private static readonly Queue<System.Action> _executionQueue = new Queue<System.Action>();

    private void Update() {
        lock (_executionQueue) {
            while (_executionQueue.Count > 0) {
                try { _executionQueue.Dequeue().Invoke(); }
                catch (System.Exception e) {
                    Debug.LogException(new System.Exception("Cannot execute queued action!", e));
                }
            }
        }
    }

    private void Start() {
        lock (_executionQueue) {
            _executionQueue.Clear();
        }
    }

    public void Enqueue(System.Action action) {
        lock (_executionQueue) { _executionQueue.Enqueue(action); }
    }
}
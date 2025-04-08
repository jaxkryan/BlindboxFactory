using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour {
    private static UnityMainThreadDispatcher _instance;
    private static readonly Queue<System.Action> _executionQueue = new Queue<System.Action>();

    public static UnityMainThreadDispatcher Instance() {
        // Ensure the instance is created only once and on the main thread.
        if (_instance == null) {
            if (Application.isPlaying) // Check if the application is playing to avoid errors in edit mode.
            {
                GameObject obj = new GameObject("UnityMainThreadDispatcher");
                _instance = obj.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(obj); // Optional: Keep it alive between scene loads.
            }
            else {
                Debug.LogError("UnityMainThreadDispatcher must be initialized during runtime.");
                return null;
            }
        }

        return _instance;
    }

    private void Awake() //Use Awake to ensure it runs before Start.
    {
        if (_instance == null) {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (_instance != this) {
            Destroy(this.gameObject); //Ensure only one instance exists.
        }
    }

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

    public void Enqueue(System.Action action) {
        lock (_executionQueue) { _executionQueue.Enqueue(action); }
    }
}
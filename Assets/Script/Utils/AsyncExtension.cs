using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Script.Utils {
    public static class AsyncExtension {
        public static IEnumerator AsCoroutine(this Task task)
        {
            if (task == null)
            {
                Debug.LogError("Task is null.");
                yield break;
            }

            while (!task.IsCompleted)
                yield return null;

            if (task.IsFaulted)
            {
                Debug.LogError($"Task threw an exception: {task.Exception?.InnerException ?? task.Exception}");
                throw task.Exception?.InnerException ?? task.Exception;
            }
            else if (task.IsCanceled)
            {
                Debug.LogWarning("Task was canceled.");
            }
        }

        public static IEnumerator AsCoroutine<T>(this Task<T> task, Action<T> onSuccess = null)
        {
            if (task == null)
            {
                Debug.LogError("Task<T> is null.");
                yield break;
            }

            while (!task.IsCompleted)
                yield return null;

            if (task.IsFaulted)
            {
                Debug.LogError($"Task<T> threw an exception: {task.Exception?.InnerException ?? task.Exception}");
                throw task.Exception?.InnerException ?? task.Exception;
            }
            else if (task.IsCanceled)
            {
                Debug.LogWarning("Task<T> was canceled.");
            }
            else
            {
                onSuccess?.Invoke(task.Result);
            }
        }
    }
}
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;

namespace Script.Controller.Permissions {
    public class PermissionHandler {
        
        public static void RequestPermissionIfNeeded(string permission, Action onDenied = null, Action onGranted = null, Action onRequestDismissed = null) {
            if (Application.platform == RuntimePlatform.Android) {
                if (!Permission.ShouldShowRequestPermissionRationale(permission)) return;
                var callbacks = new PermissionCallbacks();

                callbacks.PermissionDenied += CallbackOnDenied;
                callbacks.PermissionGranted += CallbackOnGranted;
                callbacks.PermissionRequestDismissed += CallbackOnRequestDismissed;
                    
                Permission.RequestUserPermission(permission, callbacks);
            }

            return;

            void CallbackOnDenied(string perm) {
                Debug.Log($"Permission denied: {perm}");
                onDenied?.Invoke();
            }

            void CallbackOnGranted(string perm) {
                Debug.Log($"Permission granted: {perm}");
                onGranted?.Invoke();
            }

            void CallbackOnRequestDismissed(string perm) {
                Debug.Log($"Request dismissed: {perm}");
                onRequestDismissed?.Invoke();
            }
        }

    }
}
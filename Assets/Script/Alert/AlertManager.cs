using UnityEngine;

namespace Script.Alert {
    public class AlertManager : PersistentSingleton<AlertManager> {
        [SerializeField] private AlertUI _notificationAlert;
        [SerializeField] private AlertUI _warningAlert;

        public void Raise(string header, string message, bool hasClose = true, AlertUIButtonDetails button1 = null,
            AlertUIButtonDetails button2 = null) {
        }
    }
}
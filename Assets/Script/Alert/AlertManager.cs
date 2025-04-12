using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Script.Alert {
    public class AlertManager : PersistentSingleton<AlertManager> {
        [SerializeField] private AlertUI _errorAlertPrefab;
        [SerializeField] private AlertUI _warningAlertPrefab;
        [SerializeField] private AlertUI _notificationAlertPrefab;
        private AlertUI _errorAlert;
        private AlertUI _warningAlert;
        private AlertUI _notificationAlert;
        [Header("Button Sprites")]
        [SerializeField] public Sprite Red;
        [SerializeField] public Sprite Blue;
        [SerializeField] public Sprite Green;
        [SerializeField] public Sprite Purple;
        [SerializeField] public Sprite Yellow;
        [Space]
        [SerializeField] bool _logs = false;
        private GameObject _backgroundBlocker;
        
        public event Action<AlertType, string> onAlertRaised = delegate { };

        private void Start() {
            _backgroundBlocker = CreateBlocker();
            _backgroundBlocker.gameObject.SetActive(false);
            if (_errorAlertPrefab is null) Debug.LogError("Error alert missing!");
            else {
                _errorAlert = Instantiate(_errorAlertPrefab, this.transform);
                _errorAlert.Close();
            }
            if (_warningAlertPrefab is null) Debug.LogError("Warning alert missing!");
            else {
                _warningAlert = Instantiate(_warningAlertPrefab, this.transform);
                _warningAlert.Close();
            }
            if (_notificationAlertPrefab is null) Debug.LogError("Notification alert missing!");
            else {
                _notificationAlert = Instantiate(_notificationAlertPrefab, this.transform);
                _notificationAlert.Close();
            }

        }

        private GameObject CreateBlocker() {
            if (_backgroundBlocker is not null) {
                Destroy(_backgroundBlocker.gameObject);
            }

            var blockers = GetComponentsInChildren<UIBlocker>();
            foreach (var b in blockers) {
                Destroy(b.gameObject);
            }
            
            var blocker = new GameObject("Blocker", typeof(UIBlocker), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            blocker.transform.SetParent(this.transform);

            var rect   = blocker.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            
            var img = blocker.GetComponent<Image>();
            img.color = Color.clear;
            img.raycastTarget = true;
            
            return blocker;
        }
        
        private Queue<GameAlert> _alertBackLog = new();
        private AlertUI[] _alerts => new[] { _errorAlert, _warningAlert, _notificationAlert };

        public void RaiseBackLog() {
            if (_alertBackLog is null || _alertBackLog.Count == 0) return;
            if (_alerts.Any(a => a.gameObject.activeInHierarchy)) return;
            
            var alert = _alertBackLog.Dequeue();
            Raise(alert);
        }
        
        public void Raise(GameAlert alert)
            => Raise(alert.Type, alert.Header, alert.Message, alert.HasCloseButton, alert.PauseGame, alert.OnClose, alert.Button1, alert.Button2);
        
        public void Raise(AlertType type, string header, string message, bool hasClose = true, bool pauseGame = false, Action onClose = null, [CanBeNull] AlertUIButtonDetails button1 = null,
            [CanBeNull] AlertUIButtonDetails button2 = null) {
            
            if (_alerts.Any(a => a.gameObject.activeInHierarchy)) {
                _alertBackLog.Enqueue(new GameAlert.Builder(type)
                    .WithHeader(header)
                    .WithMessage(message)
                    .HasCloseButton(hasClose)
                    .CanPauseGame(pauseGame)
                    .OnClose(onClose)
                    .WithButton1(button1)
                    .WithButton2(button2)
                    .Build());
                return;
            }
            if (_logs) Debug.Log("Raising alert " + header);
            
            var alert = type switch {
                AlertType.Error => _errorAlert,
                AlertType.Warning => _warningAlert,
                AlertType.Notification => _notificationAlert,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            
            alert.Header.text = header;
            alert.Message.text = message;
            alert.CloseButton.gameObject.SetActive(hasClose);
            if (button1 is not null) button1.DecorateButton(alert.Button1, alert);
            else alert.Button1.gameObject.SetActive(false);
            if (button2 is not null) button2.DecorateButton(alert.Button2, alert);
            else alert.Button2.gameObject.SetActive(false);

            if (pauseGame) {
                var timeScale = Time.timeScale;
                Time.timeScale = 0;

                Action resumeGame = null;
                resumeGame = () => {
                    Time.timeScale = timeScale;
                    alert.onAlertClosed -= resumeGame;
                };
                
                alert.onAlertClosed += resumeGame;
            }

            if (onClose != null) {
                Action wrapper = null;
                wrapper = () => {
                    onClose?.Invoke();
                    alert.onAlertClosed -= wrapper;
                };
                alert.onAlertClosed += wrapper;
            }

            _backgroundBlocker.gameObject.SetActive(true);
            
            alert.enabled = true;
            alert.gameObject.SetActive(true);
            onAlertRaised?.Invoke(type, alert.Header.text);
        }

        private void OnValidate() {
            if (!Red || !Blue || !Green || !Purple || !Yellow) {
                throw new NullReferenceException("Button sprite is missing!");
            }
        }
    }

    public static class AlertException {
        public static System.Exception RaiseException(this System.Exception exception, Action onClose = null) {
            var header = exception.GetType().Name;
            var message = exception.Message;
            AlertManager.Instance.Raise(AlertType.Error, header, message, pauseGame: true, onClose: onClose);
            return exception;
        }

    }

    public enum AlertType {
        Error,
        Warning,
        Notification,
    }
}
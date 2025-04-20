using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Script.Utils;
using Unity.VisualScripting;
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

        [Header("Button Sprites")] [SerializeField]
        public Sprite Red;

        [SerializeField] public Sprite Blue;
        [SerializeField] public Sprite Green;
        [SerializeField] public Sprite Purple;
        [SerializeField] public Sprite Yellow;

        public GameAlert Current { get; private set; } = null;

        [Space] [SerializeField] bool _logs = false;

        //Test
        [Space] [SerializeField] private bool _test = false;
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

        private void Update() {
            if (_test) {
                GameAlert.QuickNotification("Test", canPause: true).Raise();
                _test = false;
            }
        }

        private GameObject CreateBlocker() {
            if (_backgroundBlocker is not null) {
                Destroy(_backgroundBlocker.gameObject);
            }

            var blockers = GetComponentsInChildren<UIBlocker>();
            var slatedForDestroy = new HashSet<UIBlocker>();
            foreach (var b in blockers) {
                if (!b.TryGetComponent<RectTransform>(out var rt)) {
                    if (rt.anchorMin != Vector2.zero || rt.anchorMax != Vector2.one) slatedForDestroy.Add(b);
                    if (rt.offsetMin != Vector2.zero || rt.offsetMax != Vector2.zero) slatedForDestroy.Add(b);
                    if (rt.localScale != Vector3.one) slatedForDestroy.Add(b);
                }

                if (!b.TryGetComponent<Image>(out var im)) {
                    if (im.color != Color.clear) slatedForDestroy.Add(b);
                    if (!im.raycastTarget) slatedForDestroy.Add(b);
                }
            }

            var exempt = blockers.Except(slatedForDestroy).ToList();
            if (exempt.Any()) {
                var first = exempt.First();
                exempt.Remove(first);
                slatedForDestroy.AddRange(exempt);
                slatedForDestroy.ForEach(b => Destroy(b.gameObject));
                return first.gameObject;
            }

            var blocker = new GameObject("Blocker", typeof(UIBlocker), typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image));
            blocker.transform.SetParent(this.transform);

            var rect = blocker.GetComponent<RectTransform>();
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

        public void Raise(GameAlert alert, bool raiseAgainIfDuplicated = false) {
            // Raise(alert.Type, alert.Header, alert.Message, alert.HasCloseButton, alert.PauseGame, alert.OnClose, alert.Button1, alert.Button2);
            if (_alerts.Any(a => a.gameObject.activeInHierarchy)) {
                if ((_alertBackLog.Any(a => a == alert) || alert == Current) && raiseAgainIfDuplicated) {
                    _alertBackLog.Enqueue(alert);
                }

                return;
            }

            if (_logs) Debug.Log("Raising alert " + alert.Header);
            Current = alert;

            //Type
            var fields = alert.GetType().GetFields().ToList();
            fields.RemoveAll(f => f.Name == nameof(alert.Type));
            var raisingAlert = alert.Type switch {
                AlertType.Error => _errorAlert,
                AlertType.Warning => _warningAlert,
                AlertType.Notification => _notificationAlert,
                _ => throw new ArgumentOutOfRangeException(nameof(alert.Type), alert.Type, null)
            };


            //Header
            fields.RemoveAll(f => f.Name == nameof(alert.Header));
            raisingAlert.Header.text = alert.Header;
            //Message
            fields.RemoveAll(f => f.Name == nameof(alert.Message));
            raisingAlert.Message.text = alert.Message;
            //Close button
            fields.RemoveAll(f => f.Name == nameof(alert.HasCloseButton));
            raisingAlert.CloseButton.gameObject.SetActive(alert.HasCloseButton);
            //Button1
            fields.RemoveAll(f => f.Name == nameof(alert.Button1));
            if (alert.Button1 is not null) alert.Button1.DecorateButton(raisingAlert.Button1, raisingAlert);
            else raisingAlert.Button1.gameObject.SetActive(false);
            //Button2
            fields.RemoveAll(f => f.Name == nameof(alert.Button2));
            if (alert.Button2 is not null) alert.Button2.DecorateButton(raisingAlert.Button2, raisingAlert);
            else raisingAlert.Button2.gameObject.SetActive(false);

            //Pause Game
            fields.RemoveAll(f => f.Name == nameof(alert.PauseGame));
            if (alert.PauseGame) {
                var timeScale = Time.timeScale;
                Time.timeScale = 0;

                Action resumeGame = null;
                resumeGame = () => {
                    Time.timeScale = timeScale;
                    raisingAlert.onAlertClosed -= resumeGame;
                };

                raisingAlert.onAlertClosed += resumeGame;
            }

            //On close
            fields.RemoveAll(f => f.Name == nameof(alert.OnClose));
            if (alert.OnClose != null) {
                Action wrapper = null;
                wrapper = () => {
                    alert.OnClose?.Invoke();
                    raisingAlert.onAlertClosed -= wrapper;
                };
                raisingAlert.onAlertClosed += wrapper;
            }

            if (fields.Any())
                Debug.LogWarning(
                    $"Field(s) not used when creating alert: {string.Join(", ", fields.Select(f => f.Name))}");

            _backgroundBlocker.gameObject.SetActive(true);

            Action RemoveBlockerOnAlertClosed = null;
            RemoveBlockerOnAlertClosed = () => {
                _backgroundBlocker.gameObject.SetActive(false);
                raisingAlert.onAlertClosed -= RemoveBlockerOnAlertClosed;
            };
            raisingAlert.onAlertClosed += RemoveBlockerOnAlertClosed;

            raisingAlert.enabled = true;
            raisingAlert.gameObject.SetActive(true);
            if (raisingAlert.gameObject.TryGetComponent<DotweenAnimation>(out var anim)) {
                anim.AnimateIn();
                var messageContainer = raisingAlert.Message.transform.parent;
                if (messageContainer != null) {
                    messageContainer.localPosition.Set(messageContainer.localPosition.x, 0, messageContainer.localPosition.y);
                }
            }

            onAlertRaised?.Invoke(alert.Type, raisingAlert.Header.text);
        }

        public void Raise(AlertType type, string header, string message, bool hasClose = true, bool pauseGame = false,
            Action onClose = null,
            [CanBeNull] AlertUIButtonDetails button1 = null,
            [CanBeNull] AlertUIButtonDetails button2 = null,
            bool raiseAgainIfDuplicated = false) {
            new GameAlert.Builder(type)
                .WithHeader(header)
                .WithMessage(message)
                .WithCloseButton(hasClose)
                .CanPauseGame(pauseGame)
                .OnClose(onClose)
                .WithButton1(button1)
                .WithButton2(button2)
                .Build().Raise(raiseAgainIfDuplicated);
        }

        private void OnValidate() {
            if (!Red || !Blue || !Green || !Purple || !Yellow) {
                throw new NullReferenceException("Button sprite is missing!");
            }
        }
    }

    public enum AlertType {
        Error,
        Warning,
        Notification,
    }
}
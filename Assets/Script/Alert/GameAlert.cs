using System;
using UnityEngine;

namespace Script.Alert {
    public class GameAlert {
        public AlertType Type;
        public string Header;
        public string Message;
        public bool HasCloseButton;
        public bool PauseGame;
        public Action OnClose;
        public AlertUIButtonDetails Button1;
        public AlertUIButtonDetails Button2;

        public GameAlert(AlertType type) {
            Type = type;
        }
        
        public static bool operator ==(GameAlert a, GameAlert b) {
            if (a is null || b is null) return false;
            if (a.Type != b.Type) return false;
            if (a.Header != b.Header) return false;
            if (a.Message != b.Message) return false;
            if (a.HasCloseButton != b.HasCloseButton) return false;
            if (a.PauseGame != b.PauseGame) return false;
            if (a.OnClose != b.OnClose) return false;
            if (a.Button1 != b.Button1) return false;
            if (a.Button2 != b.Button2) return false;
            
            
            return true;
        }

        public static bool operator !=(GameAlert a, GameAlert b) {
            return !(a == b);
        }

        public static GameAlert QuickNotification(string message, string header = null, bool canPause = false, Action onClose = null) => new GameAlert.Builder(AlertType.Notification).WithHeader(header).WithMessage(message).WithCloseButton().CanPauseGame(canPause).OnClose(onClose).Build();
        public static GameAlert QuickWarning(string message, string header = null, bool canPause = true, Action onClose = null) => new GameAlert.Builder(AlertType.Notification).WithHeader(header).WithMessage(message).WithCloseButton().CanPauseGame(canPause).OnClose(onClose).Build();
        public static GameAlert QuickError(string message, string header = null, bool canPause = true, Action onClose = null) => new GameAlert.Builder(AlertType.Notification).WithHeader(header).WithMessage(message).WithCloseButton().CanPauseGame(canPause).OnClose(onClose).Build();

        public class Builder {
            private GameAlert _alert;
            public Builder(AlertType type) => _alert = new GameAlert(type);

            public GameAlert Build() {
                if (!_alert.HasCloseButton && _alert.Button1 is null & _alert.Button2 is null) {
                    Debug.LogWarning("Alert doesn't have any button");
                    WithCloseButton();
                }
                return _alert;
            }

            public Builder WithHeader(string header) {
                _alert.Header = header;
                return this;
            }

            public Builder WithMessage(string message) {
                _alert.Message = message;
                return this;
            }

            public Builder OnClose(Action onClose) {
                _alert.OnClose = onClose;
                return this;
            }

            public Builder WithCloseButton(bool hasCloseButton = true) {
                _alert.HasCloseButton = hasCloseButton;
                return this;
            }

            public Builder CanPauseGame(bool pause = true) {
                _alert.PauseGame = pause;
                return this;
            }

            public Builder WithButton1(AlertUIButtonDetails button1) {
                _alert.Button1 = button1;
                return this;
            }

            public Builder WithButton2(AlertUIButtonDetails button2) {
                _alert.Button2 = button2;
                return this;
            }
        }
    }
}
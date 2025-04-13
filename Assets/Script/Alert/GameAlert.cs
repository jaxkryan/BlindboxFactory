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
using System;
using Script.Alert;

namespace Script.Utils {

    public static class GameAlertExtensions {
        public static void Raise(this GameAlert alert, bool raiseAgainIfDuplicated = false) => AlertManager.Instance.Raise(alert, raiseAgainIfDuplicated);
        public static System.Exception RaiseException(this System.Exception exception, Action onClose = null, AlertUIButtonDetails button1 = null, AlertUIButtonDetails button2 = null) {
            var header = exception.GetType().Name;
            var message = exception.Message;
            AlertManager.Instance.Raise(new GameAlert.Builder(AlertType.Error)
                .WithHeader(header)
                .WithMessage(message)
                .WithCloseButton()
                .CanPauseGame()
                .WithButton1(button1)
                .WithButton2(button2)
                .OnClose(onClose)
                .Build());
            return exception;
        }
    }
}
using System;
using UnityEngine;

namespace Script.Alert {
    public class AlertUI : MonoBehaviour {
        
    }

    public class AlertUIButtonDetails {
        public Color Color;
        public string Text;
        public Action OnClick;
        public bool IsCloseButton;
        
        
        public static AlertUIButtonDetails CloseButton = new AlertUIButtonDetails() {
            Color = Color.red,
            Text = "Close",
            IsCloseButton = true,
        };
        public static AlertUIButtonDetails CloseApplicationButton = new AlertUIButtonDetails() {
            Color = Color.red,
            Text = "Close",
            IsCloseButton = true,
            OnClick = Application.Quit
        }; 
    }
}
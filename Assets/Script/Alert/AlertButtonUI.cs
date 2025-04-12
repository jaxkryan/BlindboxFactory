using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Script.Alert {
    public class AlertButtonUI : MonoBehaviour {
        [SerializeField] public TextMeshProUGUI Text;
        [SerializeField] public Image Background;
        [SerializeField] public Button Button;

        private void OnValidate() {
            Button = gameObject.GetComponent<Button>();
            if (Button is null) {
                throw new NullReferenceException("Game object does not have a Button component");
            }
        }
    }
    public class AlertUIButtonDetails {
        [CanBeNull] public Sprite Background;
        public string Text;
        public Color TextColor = Color.white;
        [CanBeNull] public Action OnClick;
        public bool IsCloseButton;

        public void DecorateButton(AlertButtonUI button, AlertUI alert) {
            if (button.TryGetComponent<Image>(out var image) && Background)
                image.sprite = Background;
            button.Text.text = Text;
            button.Text.color = TextColor;
            
            if (OnClick is not null)button.gameObject.GetComponent<Button>().onClick.AddListener(() => OnClick?.Invoke());
            if (IsCloseButton) {
                UnityAction listener = null;
                listener = () => {
                    alert.Close();
                    button.Button.onClick.RemoveListener(listener);
                };
                button.Button.onClick.AddListener(listener);
            }
            button.gameObject.SetActive(true);
        }
        
        
        
        public static AlertUIButtonDetails CloseButton = new AlertUIButtonDetails() {
            Text = "Close",
            IsCloseButton = true,
            Background = AlertManager.Instance.Red
        };
        public static AlertUIButtonDetails CloseApplicationButton = new AlertUIButtonDetails() {
            Text = "Quit Game",
            IsCloseButton = true,
            OnClick = Application.Quit,
            Background = AlertManager.Instance.Red
        }; 
    }
}
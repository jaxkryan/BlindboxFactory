using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Script.Alert {
    public class AlertUI : MonoBehaviour {
        [SerializeField] public Image Background;
        [SerializeField] public TextMeshProUGUI Header;
        [SerializeField] public TextMeshProUGUI Message;
        [SerializeField] public AlertButtonUI Button1;
        [SerializeField] public AlertButtonUI Button2;
        [SerializeField] public Button CloseButton;

        public event Action onAlertClosed = delegate {
            AlertManager.Instance.StartCoroutine(RaiseAlert());

            IEnumerator RaiseAlert() {
                yield return new WaitForNextFrameUnit();
                AlertManager.Instance.RaiseBackLog();
            }
        };

        public void Close() {
            Button1.Button.onClick.RemoveAllListeners();
            Button2.Button.onClick.RemoveAllListeners();
            
            this.gameObject.SetActive(false);
        }

        private void OnDisable() {
            if (!gameObject.activeInHierarchy) {
                onAlertClosed?.Invoke();
            }
        }
    }
}
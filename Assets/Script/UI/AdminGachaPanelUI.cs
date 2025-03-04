using System;
using Script.HumanResource.Administrator;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AdminGachaPanelUI : MonoBehaviour {
    [SerializeField] private bool _canClose;
    [FormerlySerializedAs("_closeButton")] [SerializeField] private Button _closeBtn;
    [SerializeField] private Button _gachaBtn;
    [SerializeField] private AdministratorGacha _adminGacha;

    public event Action<Mascot> onAdminGacha = delegate { };

    
    private void Awake() {       
        _closeBtn.gameObject.SetActive(_canClose);
    }

    private void OnValidate() {
        _closeBtn.gameObject.SetActive(_canClose);
    }

    public void Setup(AdministratorGacha gacha) => _adminGacha = gacha;
    public void Close() {
        Destroy(this.gameObject);
    }

    public void Gacha() {
        if (_adminGacha is null) {
            Debug.LogWarning("No administator gacha was selected");
            return;
        }
        _closeBtn.gameObject.SetActive(false);
        _gachaBtn.gameObject.SetActive(false);

        var pulled = _adminGacha.Pull();

        onAdminGacha?.Invoke(pulled);
    }
}

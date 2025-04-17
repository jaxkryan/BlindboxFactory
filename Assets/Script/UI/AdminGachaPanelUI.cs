using System;
using System.Collections.Generic;
using System.Linq;
using Script.Controller;
using Script.HumanResource.Administrator;
using Script.Resources;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AdminGachaPanelUI : MonoBehaviour
{
    [SerializeField] private bool _canClose;
    [FormerlySerializedAs("_closeButton")][SerializeField] private Button _closeBtn;
    [SerializeField] private Button _singlePullBtn;
    [SerializeField] private Button _tenPullBtn;
    [SerializeField] private AdministratorGacha _adminGacha;
    [SerializeField] private GameObject _gachaPanel;
    [SerializeField] private GachaRevealPanelUI _revealPanel;

    [SerializeField] private Resource _pullResource = Resource.Gold;
    [SerializeField] private long _singlePullCost = 1000;
    [SerializeField] private long _tenPullCost = 9000;

    public event Action<Mascot> onAdminGacha = delegate { };

    private void Awake()
    {
        _closeBtn.gameObject.SetActive(_canClose);

        _closeBtn.onClick.AddListener(Close);
        if (_singlePullBtn != null) _singlePullBtn.onClick.AddListener(SinglePull);
        if (_tenPullBtn != null) _tenPullBtn.onClick.AddListener(TenPull);

        // Ensure the GachaRevealPanelUI is initially inactive
        if (_revealPanel != null)
        {
            _revealPanel.gameObject.SetActive(false);
        }
    }

    private void OnValidate()
    {
        _closeBtn.gameObject.SetActive(_canClose);
    }

    private void OnDestroy()
    {
        if (_closeBtn != null) _closeBtn.onClick.RemoveListener(Close);
        if (_singlePullBtn != null) _singlePullBtn.onClick.RemoveListener(SinglePull);
        if (_tenPullBtn != null) _tenPullBtn.onClick.RemoveListener(TenPull);
    }

    public void Setup(AdministratorGacha gacha) => _adminGacha = gacha;

    public void Open()
    {
        if (_gachaPanel != null)
        {
            _gachaPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Cannot open GachaPanel: panel reference is null!");
        }
    }

    public void Close()
    {
        if (_gachaPanel != null)
        {
            _gachaPanel.SetActive(false);
        }
    }

    public void SinglePull()
    {
        if (_adminGacha is null)
        {
            Debug.LogWarning("No administrator gacha was selected");
            return;
        }

        if (!Enum.IsDefined(typeof(Resource), _pullResource))
        {
            var pulled = _adminGacha.Pull();
            onAdminGacha?.Invoke(pulled);
            ShowReveal(new List<Mascot> { pulled });
            Debug.Log("Performed a free single pull.");
            return;
        }

        if (GameController.Instance.ResourceController.TryGetAmount(_pullResource, out long amount) && amount >= _singlePullCost)
        {
            if (GameController.Instance.ResourceController.TrySetAmount(_pullResource, amount - _singlePullCost))
            {
                var pulled = _adminGacha.Pull();
                onAdminGacha?.Invoke(pulled);
                ShowReveal(new List<Mascot> { pulled });
                Debug.Log($"Performed a single pull using {_singlePullCost} {_pullResource}.");
            }
            else
            {
                Debug.LogWarning($"Failed to deduct {_singlePullCost} {_pullResource} for single pull.");
            }
        }
        else
        {
            Debug.LogWarning($"Not enough {_pullResource}! Required: {_singlePullCost}, Available: {amount}");
        }
    }

    public void TenPull()
    {
        if (_adminGacha is null)
        {
            Debug.LogWarning("No administrator gacha was selected");
            return;
        }

        if (!Enum.IsDefined(typeof(Resource), _pullResource))
        {
            var pulled = _adminGacha.PullMultiple(10).ToList();
            foreach (var item in pulled)
            {
                onAdminGacha?.Invoke(item);
            }
            ShowReveal(pulled);
            Debug.Log("Performed a free 10-pull.");
            return;
        }

        if (GameController.Instance.ResourceController.TryGetAmount(_pullResource, out long amount) && amount >= _tenPullCost)
        {
            if (GameController.Instance.ResourceController.TrySetAmount(_pullResource, amount - _tenPullCost))
            {
                var pulled = _adminGacha.PullMultiple(10).ToList();
                foreach (var item in pulled)
                {
                    onAdminGacha?.Invoke(item);
                }
                ShowReveal(pulled);
                Debug.Log($"Performed a 10-pull using {_tenPullCost} {_pullResource}.");
            }
            else
            {
                Debug.LogWarning($"Failed to deduct {_tenPullCost} {_pullResource} for 10-pull.");
            }
        }
        else
        {
            Debug.LogWarning($"Not enough {_pullResource}! Required: {_tenPullCost}, Available: {amount}");
        }
    }

    private void ShowReveal(List<Mascot> mascots)
    {
        if (_revealPanel == null)
        {
            Debug.LogWarning("Reveal panel is not assigned!");
            return;
        }

        // Activate the reveal panel and start the animation
        _revealPanel.gameObject.SetActive(true);
        _revealPanel.RevealMascots(mascots, () =>
        {
            // Deactivate the reveal panel and reopen the gacha panel after the reveal is complete
            _revealPanel.gameObject.SetActive(false);
            Open();
        });

        // Hide the gacha panel during the reveal
        //Close();
    }
}
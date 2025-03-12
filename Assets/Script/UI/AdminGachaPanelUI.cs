using System;
using Script.Controller;  // For GameController/ResourceController access
using Script.HumanResource.Administrator;
using Script.Resources;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AdminGachaPanelUI : MonoBehaviour
{
    [SerializeField] private bool _canClose;
    [FormerlySerializedAs("_closeButton")][SerializeField] private Button _closeBtn;
    [SerializeField] private Button _gachaBtn;
    [SerializeField] private AdministratorGacha _adminGacha;
    [SerializeField] private GameObject _gachaPanel;  // New panel GameObject reference

    // Serialized fields for resource-based gacha buttons
    [SerializeField] private Button _gachaGoldBtn;        // Button for Gold-based single pull
    [SerializeField] private Button _gachaGemBtn;         // Button for Gem-based single pull
    [SerializeField] private Button _gachaGold10Btn;      // Button for Gold-based 10-pull
    [SerializeField] private Button _gachaGem10Btn;       // Button for Gem-based 10-pull
    [SerializeField] private long _goldCost = 1000;       // Cost for Gold single pull
    [SerializeField] private long _gemCost = 50;          // Cost for Gem single pull
    [SerializeField] private long _goldCost10 = 9000;     // Cost for Gold 10-pull
    [SerializeField] private long _gemCost10 = 450;       // Cost for Gem 10-pull

    public event Action<Mascot> onAdminGacha = delegate { };

    private void Awake()
    {
        // Ensure the panel is hidden by default
        if (_gachaPanel != null)
        {
            _gachaPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GachaPanel is not assigned in the Inspector!");
        }

        _closeBtn.gameObject.SetActive(_canClose);

        // Setup button listeners
        _closeBtn.onClick.AddListener(Close);
        _gachaBtn.onClick.AddListener(Gacha);
        if (_gachaGoldBtn != null) _gachaGoldBtn.onClick.AddListener(GachaWithGold);
        if (_gachaGemBtn != null) _gachaGemBtn.onClick.AddListener(GachaWithGem);
        if (_gachaGold10Btn != null) _gachaGold10Btn.onClick.AddListener(GachaWithGold10);
        if (_gachaGem10Btn != null) _gachaGem10Btn.onClick.AddListener(GachaWithGem10);
    }

    private void OnValidate()
    {
        _closeBtn.gameObject.SetActive(_canClose);
    }

    private void OnDestroy()
    {
        // Clean up listeners
        _closeBtn.onClick.RemoveListener(Close);
        _gachaBtn.onClick.RemoveListener(Gacha);
        if (_gachaGoldBtn != null) _gachaGoldBtn.onClick.RemoveListener(GachaWithGold);
        if (_gachaGemBtn != null) _gachaGemBtn.onClick.RemoveListener(GachaWithGem);
        if (_gachaGold10Btn != null) _gachaGold10Btn.onClick.RemoveListener(GachaWithGold10);
        if (_gachaGem10Btn != null) _gachaGem10Btn.onClick.RemoveListener(GachaWithGem10);
    }

    public void Setup(AdministratorGacha gacha) => _adminGacha = gacha;

    // Modified Close method to deactivate the panel instead of destroying
    public void Close()
    {
        if (_gachaPanel != null)
        {
            _gachaPanel.SetActive(false);
        }
    }

    // New method to open the panel
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

    // test single pull
    public void Gacha()
    {
        if (_adminGacha is null)
        {
            Debug.LogWarning("No administrator gacha was selected");
            return;
        }

        var pulled = _adminGacha.Pull();
        onAdminGacha?.Invoke(pulled);
    }

    // test 10-pull
    public void Gacha_10()
    {
        if (_adminGacha is null)
        {
            Debug.LogWarning("No administrator gacha was selected");
            return;
        }

        var pulled = _adminGacha.PullMultiple(10);
        foreach (var item in pulled)
        {
            onAdminGacha?.Invoke(item);
        }
    }

    // Gold-based single pull
    public void GachaWithGold()
    {
        if (_adminGacha is null)
        {
            Debug.LogWarning("No administrator gacha was selected");
            return;
        }

        if (GameController.Instance.ResourceController.TryGetAmount(Resource.Gold, out long goldAmount) && goldAmount >= _goldCost)
        {
            if (GameController.Instance.ResourceController.TrySetAmount(Resource.Gold, goldAmount - _goldCost))
            {
                var pulled = _adminGacha.Pull();
                onAdminGacha?.Invoke(pulled);
            }
            else
            {
                Debug.LogWarning("Failed to deduct Gold for gacha pull");
            }
        }
        else
        {
            Debug.LogWarning($"Not enough Gold! Required: {_goldCost}, Available: {goldAmount}");
        }
    }

    // Gem-based single pull
    public void GachaWithGem()
    {
        if (_adminGacha is null)
        {
            Debug.LogWarning("No administrator gacha was selected");
            return;
        }

        if (GameController.Instance.ResourceController.TryGetAmount(Resource.Gem, out long gemAmount) && gemAmount >= _gemCost)
        {
            if (GameController.Instance.ResourceController.TrySetAmount(Resource.Gem, gemAmount - _gemCost))
            {
                var pulled = _adminGacha.Pull();
                onAdminGacha?.Invoke(pulled);
            }
            else
            {
                Debug.LogWarning("Failed to deduct Gems for gacha pull");
            }
        }
        else
        {
            Debug.LogWarning($"Not enough Gems! Required: {_gemCost}, Available: {gemAmount}");
        }
    }

    // Gold-based 10-pull
    public void GachaWithGold10()
    {
        if (_adminGacha is null)
        {
            Debug.LogWarning("No administrator gacha was selected");
            return;
        }

        if (GameController.Instance.ResourceController.TryGetAmount(Resource.Gold, out long goldAmount) && goldAmount >= _goldCost10)
        {
            if (GameController.Instance.ResourceController.TrySetAmount(Resource.Gold, goldAmount - _goldCost10))
            {
                var pulled = _adminGacha.PullMultiple(10);
                foreach (var item in pulled)
                {
                    onAdminGacha?.Invoke(item);
                }
            }
            else
            {
                Debug.LogWarning("Failed to deduct Gold for 10-pull gacha");
            }
        }
        else
        {
            Debug.LogWarning($"Not enough Gold! Required: {_goldCost10}, Available: {goldAmount}");
        }
    }

    // Gem-based 10-pull
    public void GachaWithGem10()
    {
        if (_adminGacha is null)
        {
            Debug.LogWarning("No administrator gacha was selected");
            return;
        }

        if (GameController.Instance.ResourceController.TryGetAmount(Resource.Gem, out long gemAmount) && gemAmount >= _gemCost10)
        {
            if (GameController.Instance.ResourceController.TrySetAmount(Resource.Gem, gemAmount - _gemCost10))
            {
                var pulled = _adminGacha.PullMultiple(10);
                foreach (var item in pulled)
                {
                    onAdminGacha?.Invoke(item);
                }
            }
            else
            {
                Debug.LogWarning("Failed to deduct Gems for 10-pull gacha");
            }
        }
        else
        {
            Debug.LogWarning($"Not enough Gems! Required: {_gemCost10}, Available: {gemAmount}");
        }
    }
}
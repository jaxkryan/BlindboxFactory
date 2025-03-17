using Script.HumanResource.Administrator;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MascotDetailUI : MonoBehaviour
{
    [SerializeField] private Image _portrait;
    [SerializeField] private Sprite _defaultPortrait;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _policiesText;
    [SerializeField] private TextMeshProUGUI _gradeText; // Assuming Mascot has a Grade property
    [SerializeField] private Button _closeButton;

    private void Awake()
    {
        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }
    }

    private void OnDestroy()
    {
        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveAllListeners();
        }
    }

    public void DisplayDetails(Mascot mascot)
    {
        if (mascot == null)
        {
            Debug.LogWarning("No mascot provided to display details!");
            return;
        }

        _portrait.sprite = mascot.Portrait ?? _defaultPortrait;
        _nameText.text = mascot.Name.ToString();
        _policiesText.text = mascot.Policies.Aggregate("", (current, p) => current + p.Description + "\n").TrimEnd('\n');
        _gradeText.text = $"Grade: {mascot.Grade}"; // Adjust based on your Mascot class
    }
}
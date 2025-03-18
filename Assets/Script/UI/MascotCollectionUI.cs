using System.Linq;
using Script.Controller;
using Script.HumanResource.Administrator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MascotCollectionUI : MonoBehaviour
{
    [SerializeField] private GameObject _managerPanel;           // Main panel for the UI
    [SerializeField] private Transform _mascotContainer;        // Container for mascot entries (e.g., Scroll View content)
    [SerializeField] private MascotEntryUI _mascotEntryPrefab;  // Prefab for each mascot entry
    [SerializeField] private Button _closeButton;               // Button to close the panel
    [SerializeField] private MascotDetailUI _detailPanel;       // Panel to show full mascot details

    private MascotController _mascotController;

    private void Awake()
    {
        _mascotController = GameController.Instance.MascotController;

        // Ensure the panel is hidden by default
        if (_managerPanel != null)
        {
            _managerPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("ManagerPanel is not assigned in the Inspector!");
        }

        //if (_detailPanel != null)
        //{
        //    _detailPanel.gameObject.SetActive(false);
        //}

        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(Close);
        }
    }

    private void OnDestroy()
    {
        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(Close);
        }
    }

    public void Open()
    {
        if (_managerPanel == null)
        {
            Debug.LogWarning("Cannot open MascotManagerUI: ManagerPanel is null!");
            return;
        }

        _managerPanel.SetActive(true);
        PopulateMascotList();

        // Automatically show the first mascot in the detail panel if there are any mascots
        var mascots = _mascotController.MascotsList.ToList();
        if (mascots.Count > 0)
        {
            ShowMascotDetails(mascots[0]);
        }
    }

    public void Close()
    {
        if (_managerPanel != null)
        {
            _managerPanel.SetActive(false);
        }
        //if (_detailPanel != null)
        //{
        //    _detailPanel.gameObject.SetActive(false);
        //}
    }

    private void PopulateMascotList()
    {
        // Clear existing entries
        foreach (Transform child in _mascotContainer)
        {
            Destroy(child.gameObject);
        }

        // Get all mascots from MascotController
        var mascots = _mascotController.MascotsList.ToList();
        if (mascots.Count == 0)
        {
            Debug.Log("No mascots available to display.");
            var emptyText = new GameObject("EmptyText").AddComponent<TextMeshProUGUI>();
            emptyText.transform.SetParent(_mascotContainer, false);
            emptyText.text = "No mascots in collection.";
            emptyText.alignment = TextAlignmentOptions.Center;
            return;
        }

        // Instantiate an entry for each mascot
        foreach (var mascot in mascots)
        {
            var entry = Instantiate(_mascotEntryPrefab, _mascotContainer);
            entry.Setup(mascot, this); // Pass this manager for callbacks
        }
    }

    public void ShowMascotDetails(Mascot mascot)
    {
        if (_detailPanel != null)
        {
            _detailPanel.gameObject.SetActive(true);
            _detailPanel.DisplayDetails(mascot);
        }
    }

    public void DeleteMascot(Mascot mascot)
    {
        if (_mascotController.MascotsList.Contains(mascot))
        {
           
            _mascotController.RemoveMascot(mascot);
            PopulateMascotList(); // Refresh the list

            // After deletion, show the first mascot in the list if any remain
            var remainingMascots = _mascotController.MascotsList.ToList();
            if (remainingMascots.Count > 0)
            {
                ShowMascotDetails(remainingMascots[0]);
            }
            else if (_detailPanel != null)
            {
                _detailPanel.gameObject.SetActive(false); // Hide detail panel if no mascots left
            }

            Debug.Log($"Deleted mascot: {mascot.Name}");
        }
    }
}
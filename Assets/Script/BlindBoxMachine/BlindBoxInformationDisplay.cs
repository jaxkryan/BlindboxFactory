using BuildingSystem;
using Unity.VisualScripting;
using UnityEngine;

public class BlindBoxInformationDisplay : MonoBehaviour
{
    [SerializeField]
    public static BlindBoxInformationDisplay Instance { get; private set; }
    public GameObject ChosePanel;
    public BlindBoxMachine currentBlindBoxMachine;
    public Vector3 currentCoordinate;
    public ConstructionLayer _constructionLayer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); 
            return;
        }
    }

    public BlindBoxMachine GetCurrentDisplayedObject()
    {
        return currentBlindBoxMachine;
    }

    public void SetCurrentDisplayedObject(BlindBoxMachine current)
    {
        currentBlindBoxMachine = current;
    }

    public Vector3 GetCurrentCoordinate()
    {
        return currentCoordinate;
    }

    public void SetCurrentCoordinate(Vector3 current)
    {
        currentCoordinate = current;
    }

    public void SellThis()
    {
        _constructionLayer.Stored(currentCoordinate);
        ChosePanel.SetActive(false);
    }
}

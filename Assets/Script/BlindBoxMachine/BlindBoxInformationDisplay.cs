using BuildingSystem;
using Script.Machine;
using Script.Machine.Machines.Canteen;
using Script.Machine.Machines.Generator;
using Unity.VisualScripting;
using UnityEngine;

public class BlindBoxInformationDisplay : MonoBehaviour
{
    [SerializeField]
    public static BlindBoxInformationDisplay Instance { get; private set; }
    public GameObject ChosePanelAll;
    public GameObject ChosePanelBB;
    public MachineBase currentMachine;
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

    public void SellThis()
    {
        _constructionLayer.Stored(currentCoordinate);
        ChosePanelAll.SetActive(false);
        ChosePanelBB.SetActive(false);
    }
}

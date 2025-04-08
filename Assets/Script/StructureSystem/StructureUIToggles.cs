using Script.Machine;
using UnityEngine;

public class StructureUIToggles : MonoBehaviour
{
    [SerializeField] private GameObject ChosePanelAll;
    [SerializeField] private GameObject ChosePanelBB;
    [SerializeField] private GameObject CraftPanel;
    [SerializeField] private GameObject InformationPanel;
    [SerializeField] private GameObject ExitPanel;

    private MachineBase currentMachine;

    public StructureUIToggles Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        currentMachine = BlindBoxInformationDisplay.Instance.currentMachine;
    }

    public void turnOnCraftPanel()
    {
        OnEnable();
        try
        {

            CraftPanel.SetActive(true);
            ChosePanelAll.SetActive(false);
            ChosePanelBB.SetActive(false);
        }
        catch
        {

        }
    }

    public void turnOnInformationPanel()
    {
        OnEnable();
        try
        {

            InformationPanel.SetActive(true);
            ChosePanelAll.SetActive(false);
            ChosePanelBB.SetActive(false);
        }
        catch
        {

        }
    }

    public void exitsAllPanel()
    {
        OnEnable();
        try
        {
            CraftPanel.SetActive(false);
            ChosePanelAll.SetActive(false);
            ChosePanelBB.SetActive(false);
            InformationPanel.SetActive(false);
            ExitPanel.SetActive(false);
        }
        catch
        {

        }
    }

    public void backToChosePanelGeneral()
    {
        OnEnable();
        if (currentMachine is BlindBoxMachine)
        {
            backToChosePanelBB();
        }
        else
        {
            backToChosePanelAll();
        }
    }


    public void backToChosePanelAll()
    {
        OnEnable();
        try
        {
            CraftPanel.SetActive(false);
            ChosePanelAll.SetActive(true);
            InformationPanel.SetActive(false);
            ExitPanel.SetActive(true);
        }
        catch
        {

        }
    }

    public void backToChosePanelBB()
    {
        OnEnable();
        try
        {
            CraftPanel.SetActive(false);
            ChosePanelBB.SetActive(true);
            InformationPanel.SetActive(false);
            ExitPanel.SetActive(true);
        }
        catch
        {

        }
    }
}
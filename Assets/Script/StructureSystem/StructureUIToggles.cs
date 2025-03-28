using UnityEngine;

public class StructureUIToggles : MonoBehaviour
{
    [SerializeField] private GameObject ChosePanelAll;
    [SerializeField] private GameObject ChosePanelBB;
    [SerializeField] private GameObject CraftPanel;
    [SerializeField] private GameObject InformationPanel;
    [SerializeField] private GameObject ExitPanel;

    public StructureUIToggles Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void turnOnCraftPanel()
    {
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

    public void backToChosePanelAll()
    {
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

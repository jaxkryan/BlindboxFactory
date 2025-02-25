using UnityEngine;

public class StructureUIToggles : MonoBehaviour
{
    [SerializeField] private GameObject ChosePanel;
    [SerializeField] private GameObject CraftPanel;
    [SerializeField] private GameObject InformationPanel;
    [SerializeField] private GameObject UpgradePanel;

    public void turnOnCraftPanel()
    {
        try
        {

            CraftPanel.SetActive(true);
            ChosePanel.SetActive(false);
        }
        catch
        {

        }
    }

    public void turnOnUpgradePanel()
    {
        try
        {

            UpgradePanel.SetActive(true);
            ChosePanel.SetActive(false);
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
            ChosePanel.SetActive(false);
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
            ChosePanel.SetActive(false);
            InformationPanel.SetActive(false);
            UpgradePanel.SetActive(false);
        }
        catch
        {

        }
    }

    public void backToChosePanel()
    {
        try
        {
            CraftPanel.SetActive(false);
            ChosePanel.SetActive(true);
            InformationPanel.SetActive(false);
            UpgradePanel.SetActive(false);
        }
        catch
        {

        }
    }
}

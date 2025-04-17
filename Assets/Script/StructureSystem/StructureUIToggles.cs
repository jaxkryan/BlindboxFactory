using System.Linq;
using Script.Machine;
using UnityEngine;

public class StructureUIToggles : PersistentSingleton<StructureUIToggles> 
{
    [SerializeField] private GameObject ChosePanelAll;
    [SerializeField] private GameObject ChosePanelBB;
    [SerializeField] private GameObject CraftPanel;
    [SerializeField] private GameObject InformationPanel;
    [SerializeField] private GameObject ExitPanel;

    public void turnOnCraftPanel() {
        ExitsAllPanel(ExitPanel);
        CraftPanel.SetActive(true);
    }

    public void turnOnInformationPanel() {
        ExitsAllPanel(ExitPanel);
        InformationPanel.SetActive(true);
    }

    public void exitsAllPanel() => ExitsAllPanel();

    private void ExitsAllPanel(params GameObject[] excepts) {
        var panels = new[] { CraftPanel, ChosePanelAll, ChosePanelBB, InformationPanel, ExitPanel };
        var ex = panels.Where(excepts.Contains);
        try { panels.Except(ex).ForEach(p => p?.SetActive(false)); }
        catch (System.Exception e) { Debug.LogError(e); }
    }

    public void backToChosePanelGeneral() {
        ExitsAllPanel(ExitPanel);
        if (BlindBoxInformationDisplay.Instance.currentMachine is BlindBoxMachine) { backToChosePanelBB(); }
        else { backToChosePanelAll(); }
    }


    public void backToChosePanelAll() {
        ExitsAllPanel(ExitPanel);
        ChosePanelAll.SetActive(true);
    }

    public void backToChosePanelBB() {
        ExitsAllPanel(ExitPanel);
        ChosePanelBB.SetActive(true);
    }
}
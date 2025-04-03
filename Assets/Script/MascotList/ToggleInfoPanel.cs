using UnityEngine;

public class InfoToggleUI : MonoBehaviour
{
    public GameObject infoPanel;

    public void ToggleInfoPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(!infoPanel.activeSelf);
        }
    }
}
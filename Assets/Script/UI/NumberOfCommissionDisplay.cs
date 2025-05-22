using Script.Controller;
using Script.Controller.Commission;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberOfCommissionDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text m_Text;
    [SerializeField] Image m_Image;
    void Start()
    {
        numberOfCommissionDisplay();
        GameController.Instance.CommissionController.OnCommissionChanged += UpdateCommissionDisplay;
    }

    private void UpdateCommissionDisplay()
    {
        numberOfCommissionDisplay();
    }

    public void numberOfCommissionDisplay()
    {
        int com = GameController.Instance.CommissionController.Commissions.Count;
        if (com > 0)
        {
            m_Text.text = com.ToString();
        }
        else
        {
            m_Text.gameObject.SetActive(false);
            m_Image.gameObject.SetActive(false);
        }
    }
}

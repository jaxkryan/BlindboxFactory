using UnityEngine;
using UnityEngine.UI;

public class SliderButtonFix : MonoBehaviour
{
    [SerializeField]public Button targetButton;
    private bool isSliderActive = false;

    public void BlockButton()
    {
        isSliderActive = true;
    }

    public void UnblockButton()
    {
        isSliderActive = false;
    }

    void Update()
    {
        if (isSliderActive)
        {
            targetButton.interactable = false; 
        }
        else
        {
            targetButton.interactable = true; 
        }
    }
}

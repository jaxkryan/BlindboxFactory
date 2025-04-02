using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlindBoxMachineUIProgessDisplay : PersistentSingleton<BlindBoxMachineUIProgessDisplay>
{

    [SerializeField] float value = 0f;
    [SerializeField] Slider uiProgess;
    [SerializeField] Slider uiAmount;
    [SerializeField] TMP_Text textAmount;
    [SerializeField] BlindBoxMachine blindBoxMachine;

    private void Start()
    {
        UpdateUIProgress(blindBoxMachine.CurrentProgress);
        blindBoxMachine.onProgress += UpdateUIProgress;
    }

    private void UpdateUIProgress(float progress)
    {
        var blindbox = (BlindBox)blindBoxMachine.Product;
        if (blindBoxMachine.amount != 0 && blindbox.BoxTypeName != BoxTypeName.Null)
        {
            uiProgess.value = blindBoxMachine.CurrentProgress / blindBoxMachine.MaxProgress;
            uiAmount.value = blindBoxMachine.amount;
            textAmount.text = blindBoxMachine.amount + " / " + blindBoxMachine.maxAmount;
        }
        else
        {
            uiProgess.value = 0;
            uiAmount.value = 0;
            textAmount.text = " 0 / " + blindBoxMachine.maxAmount;
        }
    }
}

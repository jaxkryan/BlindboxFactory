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
        blindBoxMachine.onProgress += UpdateUIProgress;
    }

    private void UpdateUIProgress(float progress)
    {
        uiProgess.value = blindBoxMachine.CurrentProgress / blindBoxMachine.MaxProgress;
        uiAmount.value = blindBoxMachine.amount;
        textAmount.text = blindBoxMachine.amount + " / " + blindBoxMachine.maxAmount;
        //textAmount.text = (blindBoxMachine.CurrentProgress / blindBoxMachine.MaxProgress).ToString();
    }
}

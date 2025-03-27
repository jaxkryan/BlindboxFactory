using UnityEngine;
using UnityEngine.UI;

public class BlindBoxMachineUIProgessDisplay : PersistentSingleton<BlindBoxMachineUIProgessDisplay>
{

    [SerializeField] float value = 0f;
    [SerializeField] Slider uiProgess;
    [SerializeField] BlindBoxMachine blindBoxMachine;

    private void Update()
    {
        value = blindBoxMachine.CurrentProgress / blindBoxMachine.MaxProgress;
        uiProgess.value = value;
    }
}

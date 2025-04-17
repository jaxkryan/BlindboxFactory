using Script.Machine.MachineDataGetter;
using Script.Machine;
using UnityEngine;

public class MachineDisplayManager : MonoBehaviour
{
    public InformationPanel panel;
    public MachineBase targetMachine;

    void Start()
    {
        targetMachine = BlindBoxInformationDisplay.Instance.currentMachine;
        if (targetMachine != null)
        {
            MachineData machineData = MachineData.Create(panel, targetMachine);
            machineData.Draw();
        }
        else
        {
            Debug.LogError("No machine assigned!");
        }
    }

    private void OnEnable()
    {
        Start();
    }
}

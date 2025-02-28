using Unity.VisualScripting;
using UnityEngine;

public class BlindBoxInformationDisplay : MonoBehaviour
{
    public static BlindBoxInformationDisplay Instance { get; private set; }
    public GameObject ChosePanel;
    private BlindBoxMachine currentBlindBoxMachine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public BlindBoxMachine GetCurrentDisplayedObject()
    {
        return currentBlindBoxMachine;
    }

    public void SetCurrentDisplayedObject(BlindBoxMachine current)
    {
        Debug.Log("Setting current BlindBoxMachine: " + current);
        currentBlindBoxMachine = current;
    }

}

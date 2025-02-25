using UnityEngine;

public class WireTaskMain : MonoBehaviour
{
    static public WireTaskMain Instance;

    public int switchCount;
    public GameObject winText;
    private int oncount = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void SwitchChange(int points)
    {
        oncount += points;
        if(oncount == switchCount)
        {
            winText.SetActive(true);
        }
    }
}

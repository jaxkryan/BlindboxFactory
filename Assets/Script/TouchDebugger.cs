using UnityEngine;

public class TouchDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Touch detected! Total Touches: " + Input.touchCount);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class EnableAlphaRaycast : MonoBehaviour
{
    void Start()
    {
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.alphaHitTestMinimumThreshold = 0.1f;
        }
    }
}

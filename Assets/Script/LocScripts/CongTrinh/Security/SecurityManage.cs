using UnityEngine;
using UnityEngine.UI;

public class SecurityManage : MonoBehaviour
{
    public float numberOfSecurity = 2;
    public float energyConsume = 5;
    [Header("UI Elements")]
    // Hiển thị số lượng bữa ăn
    public Text securityText; // Hiển thị số lượng công nhân trong canteen
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        securityText.text = ("Security Man:") + numberOfSecurity.ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }
}

using UnityEngine;

public class ElectricMachineManage : MonoBehaviour
{
    public GameObject electricMachinePrefab;    // Prefab của máy điện
    public Transform[] electricMachinePositions; // Mảng chứa các vị trí đặt máy điện

    private void Start()
    {
        // Khởi tạo các instance của máy điện tại các vị trí cho trước
        foreach (Transform position in electricMachinePositions)
        {
            Instantiate(electricMachinePrefab, position.position, position.rotation);
        }
    }
}

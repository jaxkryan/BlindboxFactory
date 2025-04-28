using BuildingSystem.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OnlineMachineBuilder : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap; // Your isometric tilemap
    [SerializeField] private List<BuildableItem> buildableItems; // Assign your BuildableItem list here


    MachineControllerLoader machineControllerLoader = new();
    public void BuildFromFirebaseData(List<MachineControllerLoader.MachineBaseData> machines)
    {
        foreach (var machine in machines)
        {
            if (string.IsNullOrEmpty(machine.PrefabName))
            {
                Debug.LogWarning("Machine prefab name is null or empty.");
                continue;
            }

            // Search BuildableItem by GameObject name
            BuildableItem foundItem = buildableItems.Find(item => item.gameObject.name == machine.PrefabName);

            if (foundItem != null)
            {
                // Instantiate the object
                GameObject instance = Instantiate(foundItem.gameObject);

                // Convert the world position to a tilemap cell position
                Vector3 worldPos = new Vector3(machine.Position.x, machine.Position.y, machine.Position.z);
                Vector3Int cellPos = tilemap.WorldToCell(worldPos);

                // Set the object position to the center of the tile
                instance.transform.position = tilemap.GetCellCenterWorld(cellPos);

                Debug.Log($"Built {machine.PrefabName} at {cellPos}");
            }
            else
            {
                Debug.LogWarning($"No BuildableItem found matching prefab name: {machine.PrefabName}");
            }
        }
    }
}

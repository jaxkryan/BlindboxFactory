using BuildingSystem.Models;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OnlineMachineBuilder : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap; // Your isometric tilemap
    [SerializeField] private List<BuildableItem> buildableItems; // Assign your BuildableItem list here


    MachineControllerLoader machineControllerLoader;
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
            BuildableItem foundItem = buildableItems.Find(item => item.Name == machine.PrefabName);

            if (foundItem != null)
            {
                // Check if BuildableItem has a Prefab reference
                GameObject prefab = foundItem.gameObject; // Assume BuildableItem has a Prefab field
                if (prefab == null)
                {
                    Debug.LogWarning($"BuildableItem {machine.PrefabName} has no Prefab assigned.");
                    continue;
                }

                // Create a new empty GameObject
                GameObject instance = new GameObject(machine.PrefabName);

                instance.transform.localScale = prefab.transform.localScale;
                // Add SpriteRenderer component
                SpriteRenderer spriteRenderer = instance.AddComponent<SpriteRenderer>();

                // Try to get the SpriteRenderer from the prefab
                SpriteRenderer originalRenderer = prefab.GetComponent<SpriteRenderer>();
                if (originalRenderer != null)
                {
                    spriteRenderer.sprite = originalRenderer.sprite;
                    spriteRenderer.sortingOrder = originalRenderer.sortingOrder;
                }
                else
                {
                    Debug.LogWarning($"Prefab {machine.PrefabName} has no SpriteRenderer.");
                }

                // Add Animator component (optional)
                Animator originalAnimator = prefab.GetComponent<Animator>();
                if (originalAnimator != null)
                {
                    Animator animator = instance.AddComponent<Animator>();
                    animator.runtimeAnimatorController = originalAnimator.runtimeAnimatorController;
                }

                // Position the object
                Vector3 worldPos = new Vector3(machine.Position.x, machine.Position.y, machine.Position.z);
                Vector3Int cellPos = tilemap.WorldToCell(worldPos);
                instance.transform.position = tilemap.GetCellCenterWorld(cellPos);

                Debug.Log($"Built simple {machine.PrefabName} at {cellPos}");
            }
            else
            {
                Debug.LogWarning($"No BuildableItem found matching prefab name: {machine.PrefabName}");
            }
        }
    }
}

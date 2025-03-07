using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeList : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab; // Assign the ObjectItem prefab
    [SerializeField] private Transform contentPanel; // Assign the Content panel inside the Scroll View

    [SerializeField] private List<GameObject> objects = new List<GameObject>{};

    void Start()
    {
        PopulateList();
    }

    void PopulateList()
    {
        foreach (GameObject obj in objects)
        {
            GameObject newItem = Instantiate(itemPrefab, contentPanel);
            newItem.GetComponentInChildren<TMP_Text>().text = obj.name;
            newItem.GetComponentInChildren<Image>().sprite = obj.GetComponent<SpriteRenderer>().sprite;

            // Add a button click event
            newItem.GetComponent<Button>().onClick.AddListener(() => OnItemClick(obj.name));
        }
    }

    void OnItemClick(string objName)
    {
        Debug.Log("Clicked on: " + objName);
    }
}

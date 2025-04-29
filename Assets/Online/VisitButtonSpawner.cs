using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;

public class VisitButtonSpawner : MonoBehaviour
{
    public GameObject buttonPrefab; // Assign your Button Prefab here in Inspector
    public Transform contentParent; // Assign the "Content" GameObject under ScrollView here
    public string sceneToLoad = "YourSceneName"; // Set your scene to load

    void Start()
    {
        LoadAllUserIds();
    }

    void LoadAllUserIds()
    {
        FirebaseDatabase.DefaultInstance.GetReference("users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    foreach (var childSnapshot in snapshot.Children)
                    {
                        string userId = childSnapshot.Key;
                        CreateVisitButton(userId);
                    }
                }
                else
                {
                    Debug.LogWarning("No users found in the database.");
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve users: " + task.Exception);
            }
        });
    }

    void CreateVisitButton(string userId)
    {
        GameObject buttonObj = Instantiate(buttonPrefab, contentParent);
        VisitButton visitButton = buttonObj.GetComponent<VisitButton>();

        if (visitButton != null)
        {
            visitButton.userIdToLoad = userId;
            visitButton.sceneToLoad = sceneToLoad;
        }
        //Text buttonText = buttonObj.GetComponentInChildren<Text>();
        //if (buttonText != null)
        //{
        //    buttonText.text = $"Visit {userId}";
        //}
    }
}

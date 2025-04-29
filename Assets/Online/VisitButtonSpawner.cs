using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VisitButtonSpawner : MonoBehaviour
{
    public GameObject buttonPrefab; // Assign your Button Prefab here
    public Transform contentParent; // ScrollView Content holder
    public string sceneToLoad = "YourSceneName"; // Scene to visit

    void Start()
    {
        LoadAllUserIds();
    }

    void LoadAllUserIds()
    {
        FirebaseDatabase.DefaultInstance.GetReference("users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    foreach (var childSnapshot in snapshot.Children)
                    {
                        string userId = childSnapshot.Key;
                        LoadPlayerNameAndCreateButton(userId);
                    }
                }
                else
                {
                    Debug.LogWarning("No users found in database.");
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve users: " + task.Exception);
            }
        });
    }

    void LoadPlayerNameAndCreateButton(string userId)
    {
        string path = $"users/{userId}/PlayerData";
        FirebaseDatabase.DefaultInstance.GetReference(path).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            string playerName = "Guest"; // Default if missing or error

            if (task.IsCompletedSuccessfully)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists && snapshot.Value != null)
                {
                    var json = snapshot.GetRawJsonValue();
                    try
                    {
                        PlayerData data = JsonUtility.FromJson<PlayerData>(json);
                        if (!string.IsNullOrEmpty(data.PlayerName))
                        {
                            playerName = data.PlayerName;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Failed to parse PlayerData for {userId}: {e.Message}");
                    }
                }
            }

            CreateVisitButton(userId, playerName);
        });
    }

    void CreateVisitButton(string userId, string playerName)
    {
        GameObject buttonObj = Instantiate(buttonPrefab, contentParent);
        VisitButton visitButton = buttonObj.GetComponent<VisitButton>();

        if (visitButton != null)
        {
            visitButton.userIdToLoad = userId;
            visitButton.sceneToLoad = sceneToLoad;
        }

        TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = $"{playerName}";
        }
    }

    [System.Serializable]
    public class PlayerData
    {
        public string Id;
        public string PlayerName;
        public string FirstLogin;
        public string LastLogin;
    }
}

using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            Debug.Log($"[LoadPlayerName] Getting data from path: {path}");

            if (task.IsCompletedSuccessfully)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log($"[LoadPlayerName] Task completed successfully. Exists: {snapshot.Exists}, Value: {snapshot.Value}");

                if (snapshot.Exists && snapshot.Value != null)
                {
                    try
                    {
                        // Firebase data might return a stringified JSON
                        string jsonString = snapshot.Value.ToString();
                        Debug.Log($"[LoadPlayerName] Retrieved stored string: {jsonString}");

                        // Parse the stringified JSON into a JObject
                        JObject jObject = JObject.Parse(jsonString);

                        // Deserialize the inner object (PlayerData)
                        PlayerData data = jObject.ToObject<PlayerData>();
                        Debug.Log($"[LoadPlayerName] Parsed PlayerName: {data.PlayerName}");

                        if (!string.IsNullOrEmpty(data.PlayerName))
                        {
                            playerName = data.PlayerName;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[LoadPlayerName] Failed to parse PlayerData: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[LoadPlayerName] Snapshot does not exist or value is null.");
                }
            }
            else
            {
                Debug.LogWarning($"[LoadPlayerName] Task failed: {task.Exception?.Message}");
            }

            Debug.Log($"[LoadPlayerName] Final PlayerName: {playerName}");
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
            visitButton.userNameToLoad = playerName;
            visitButton.sceneToLoad = sceneToLoad;
        }

        TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = $"{playerName}";
        }
    }
}

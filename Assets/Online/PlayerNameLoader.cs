using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameLoader : MonoBehaviour
{
    public TMP_Text factoryText; // Assign your Text UI here

    void Start()
    {
        string userId = "local_user"; // default fallback
        if (!string.IsNullOrEmpty(UserIdHolder.UserId))
        {
            userId = UserIdHolder.UserId;
        }

        LoadPlayerName(userId);
    }

    void LoadPlayerName(string userId)
    {
        string path = $"users/{userId}/PlayerData";
        Debug.Log($"Loading player data from: {path}");

        FirebaseDatabase.DefaultInstance.GetReference(path).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists && snapshot.Value != null)
                {
                    string jsonString = snapshot.GetRawJsonValue();
                    Debug.Log($"Raw JSON: {jsonString}");

                    try
                    {
                        PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(jsonString);

                        string playerName = string.IsNullOrEmpty(playerData.PlayerName) ? "Guest" : playerData.PlayerName;
                        factoryText.text = $"{playerName}'s factory";
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Failed to parse PlayerData JSON: " + e.Message);
                        factoryText.text = "Guest's factory";
                    }
                }
                else
                {
                    Debug.LogWarning("Player data snapshot does not exist.");
                    factoryText.text = "Guest's factory";
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve player data: " + task.Exception);
                factoryText.text = "Guest's factory";
            }
        });
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

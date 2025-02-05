using GooglePlayGames;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine.SocialPlatforms;
using System.Text;
using UnityEngine;
using GooglePlayGames.BasicApi;
using System;

public class GooglePlaySaveManager : MonoBehaviour
{
    public string googleId => Social.localUser.id;
    public string userId => GetShortUserId(googleId).ToString();
 
    public static int GetShortUserId(string googleId)
    {
        unchecked
        {
            const int prime = 16777619;
            int hash = (int)2166136261;
            foreach (char c in googleId)
            {
                hash ^= c;
                hash *= prime;
            }

            return Math.Abs(hash) % 1000000000; // Ensure it's a positive 9-digit number
        }
    }

    public void SaveToCloud(int score, Vector3 position)
    {
        if (!Social.localUser.authenticated)
        {
            Debug.Log("Error: Not authenticated with Google Play when saving!");
            return;
        }

        DataToSave data = new DataToSave { score = score, position = position };
        string saveData = JsonUtility.ToJson(data);
        byte[] dataBytes = Encoding.UTF8.GetBytes(saveData);

        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
            "savefile",
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription("Updated score: " + score)
                        .Build();

                    PlayGamesPlatform.Instance.SavedGame.CommitUpdate(game, update, dataBytes, (commitStatus, meta) =>
                    {
                        if (commitStatus == SavedGameRequestStatus.Success)
                        {
                            Debug.Log("Game Saved! Score: " + score);
                        }
                        else
                        {
                            Debug.Log("Error: Failed to save game.");
                        }
                    });
                }
                else
                {
                    Debug.Log("Error: Failed to open save file.");
                }
            }
        );
    }

    public void LoadFromCloud(System.Action<DataToSave> onLoadComplete)
    {
        if (!Social.localUser.authenticated)
        {
            Debug.Log("Error: Not authenticated with Google Play when loading!");
            return;
        }

        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
            "savefile",
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(game, (readStatus, data) =>
                    {
                        if (readStatus == SavedGameRequestStatus.Success)
                        {
                            string saveData = Encoding.UTF8.GetString(data);
                            DataToSave loadedData = JsonUtility.FromJson<DataToSave>(saveData);
                            onLoadComplete?.Invoke(loadedData);
                        }
                        else
                        {
                            Debug.Log("Error: Failed to read save data.");
                        }
                    });
                }
                else
                {
                    Debug.Log("Error: Failed to open save file.");
                }
            }
        );
    }
}
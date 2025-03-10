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

public static string GetShortUserId(string googleId)
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

        int result = Math.Abs(hash) % 1000000000; // Ensure it's within 9 digits
        return result.ToString("D9"); // Ensure it's always 9 digits with leading zeros
    }
}
    //public void SaveToCloud(int score, Vector3 position)
    //{
    //    ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
    //    if (scoreManager != null)
    //    {
    //        scoreManager.Log("Save to cloud is running");
    //    }
    //    else
    //    {
    //        Debug.LogError("ScoreManager not found in the scene!");
    //    }

    //    if (!Social.localUser.authenticated)
    //    {
    //        scoreManager.Log("Error: Not authenticated with Google Play when saving!");
    //        return;
    //    }

    //    DataToSave data = new DataToSave { score = score, position = position };
    //    string saveData = JsonUtility.ToJson(data);
    //    byte[] dataBytes = Encoding.UTF8.GetBytes(saveData);

    //    PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
    //        "bbf_save",
    //        DataSource.ReadCacheOrNetwork,
    //        ConflictResolutionStrategy.UseLongestPlaytime,
    //        (status, game) =>
    //        {
    //            if (status == SavedGameRequestStatus.Success)
    //            {
    //                // First, close any previously opened file before writing
    //                PlayGamesPlatform.Instance.SavedGame.CommitUpdate(game, new SavedGameMetadataUpdate.Builder().Build(), new byte[0], (commitStatus, meta) =>
    //                {
    //                    if (commitStatus == SavedGameRequestStatus.Success)
    //                    {
    //                        // Now reopen and write the new data
    //                        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
    //                            "bbf_save",
    //                            DataSource.ReadCacheOrNetwork,
    //                            ConflictResolutionStrategy.UseLongestPlaytime,
    //                            (reopenStatus, reopenedGame) =>
    //                            {
    //                                if (reopenStatus == SavedGameRequestStatus.Success)
    //                                {
    //                                    SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
    //                                        .WithUpdatedDescription("Updated score: " + score)
    //                                        .Build();

    //                                    PlayGamesPlatform.Instance.SavedGame.CommitUpdate(reopenedGame, update, dataBytes, (finalStatus, finalMeta) =>
    //                                    {
    //                                        if (finalStatus == SavedGameRequestStatus.Success)
    //                                        {
    //                                            scoreManager.Log("Game Saved! Score: " + score);
    //                                        }
    //                                        else
    //                                        {
    //                                            scoreManager.Log("Error: Failed to save game.");
    //                                        }
    //                                    });
    //                                }
    //                                else
    //                                {
    //                                    scoreManager.Log("Error: Failed to reopen save file.");
    //                                }
    //                            }
    //                        );
    //                    }
    //                    else
    //                    {
    //                        scoreManager.Log("Error: Failed to close previous save file.");
    //                    }
    //                });
    //            }
    //            else
    //            {
    //                scoreManager.Log("Error: Failed to open save file at save.");
    //            }
    //        }
    //    );
    //}


    //public void LoadFromCloud(System.Action<DataToSave> onLoadComplete)
    //{
    //    ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
    //    if (scoreManager != null)
    //    {
    //        scoreManager.Log("load from cloud is running");
    //    }
    //    else
    //    {
    //        Debug.LogError("ScoreManager not found in the scene!");
    //    }
    //    if (!Social.localUser.authenticated)
    //    {
    //        scoreManager.Log("Error: Not authenticated with Google Play when loading!");
    //        return;
    //    }

    //    PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
    //        "bbf_save",
    //        DataSource.ReadCacheOrNetwork,
    //        ConflictResolutionStrategy.UseLongestPlaytime,
    //        (status, game) =>
    //        {
    //            if (status == SavedGameRequestStatus.Success)
    //            {
    //                PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(game, (readStatus, data) =>
    //                {
    //                    if (readStatus == SavedGameRequestStatus.Success)
    //                    {
    //                        string saveData = Encoding.UTF8.GetString(data);
    //                        DataToSave loadedData = JsonUtility.FromJson<DataToSave>(saveData);
    //                        onLoadComplete?.Invoke(loadedData);
    //                    }
    //                    else
    //                    {
    //                        scoreManager.Log("Error: Failed to read save data.");
    //                    }
    //                });
    //            }
    //            else
    //            {
    //                scoreManager.Log("Error: Failed to open save file at load.");
    //            }
    //        }
    //    );
    //}
}

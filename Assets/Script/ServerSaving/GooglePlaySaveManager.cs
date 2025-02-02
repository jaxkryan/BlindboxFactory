using GooglePlayGames;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine.SocialPlatforms;
using System.Text;
using UnityEngine;
using GooglePlayGames.BasicApi;

public class GooglePlaySaveManager : MonoBehaviour
{
    public string userId => Social.localUser.id;

    public void SaveToCloud(int score, Vector3 position)
    {
        if (!Social.localUser.authenticated)
        {
            Debug.Log("Error: Not authenticated with Google Play when saving!");
            return;
        }

        DataToSave data = new DataToSave { score = score, x = position.x, y = position.y, z = position.z };
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
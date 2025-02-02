using UnityEngine;
using TMPro;
using System;
using System.Text;
using GooglePlayGames;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine.SocialPlatforms;
using Firebase.Database;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using GooglePlayGames.BasicApi;

[Serializable]
public class DataToSave
{
    public string userName;
    public float x, y, z;
    public int score;
}

public class ScoreManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text debugText;
    public GameObject rotatingObject;
    public Transform movableObject;
    public float rotationSpeed = 100f;
    private int score = 0;
    private Vector3 objectPosition;
    private bool isDragging = false;
    private Vector3 offset;

    DatabaseReference dbRef;
    public string userId => Social.localUser.id;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        if (GooglePlayManager.Instance != null && GooglePlayManager.Instance.IsSignedIn)
        {
            LoadFromCloud();
        }
        else
        {
            GooglePlayManager.Instance.OnSignInComplete += LoadFromCloud;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsClickOnUI())
            {
                IncreaseScore();
            }
            else if (IsClickOnUI())
            {
                Log("Click on save button");
                SaveGame();
            }
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null && hit.transform == movableObject)
            {
                Debug.Log("Dragging Started!");
                isDragging = true;
                offset = (Vector3)mousePos - movableObject.position;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            movableObject.position = (Vector3)mousePos - offset;
        }
    }

    bool IsClickOnUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("UI"))
            {
                return true;
            }
        }
        return false;
    }

    void IncreaseScore()
    {
        score++;
        scoreText.text = score.ToString();
    }

    public void SaveGame()
    {
        Log("Saving game...");
        SaveToCloud();
        SaveToFirebase();
    }

    void SaveToCloud()
    {
        if (!Social.localUser.authenticated)
        {
            Log("Error: Not authenticated with Google Play when saving!");
            return;
        }

        objectPosition = movableObject.position;
        DataToSave data = new DataToSave { score = score, x = objectPosition.x, y = objectPosition.y, z = objectPosition.z };
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
                            Log("Game Saved! Score: " + score);
                        }
                        else
                        {
                            Log("Error: Failed to save game.");
                        }
                    });
                }
                else
                {
                    Log("Error: Failed to open save file.");
                }
            }
        );
    }

    void SaveToFirebase()
    {
        DataToSave data = new DataToSave { userName = userId, score = score, x = movableObject.position.x, y = movableObject.position.y, z = movableObject.position.z };
        string json = JsonUtility.ToJson(data);
        dbRef.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Log("Error: Failed to save data to Firebase.");
            }
            else if (task.IsCompleted)
            {
                Log("Data saved to Firebase.");
            }
        });
    }

    void LoadFromCloud()
    {
        if (!Social.localUser.authenticated)
        {
            Log("Error: Not authenticated with Google Play when loading!");
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
                            score = loadedData.score;
                            movableObject.position = new Vector3(loadedData.x, loadedData.y, loadedData.z);
                            scoreText.text = score.ToString();
                            Log("Game Loaded! Score: " + score);
                        }
                        else
                        {
                            Log("Error: Failed to read save data.");
                        }
                    });
                }
                else
                {
                    Log("Error: Failed to open save file.");
                }
            }
        );
    }

    void Log(string message)
    {
        Debug.Log(message);
        if (debugText != null)
        {
            debugText.text = message;
        }
    }
}

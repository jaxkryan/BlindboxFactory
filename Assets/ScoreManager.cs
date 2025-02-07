using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;
// using Unity.Android.Gradle.Manifest;
using System.IO;
using System.Collections;
public class ScoreManager : MonoBehaviour
{
    private GooglePlaySaveManager googlePlaySaveManager;
    private FirebaseSaveManager firebaseSaveManager;

    public TMP_Text scoreText;
    public TMP_Text debugText;
    public Transform movableObject;
    public GameObject LDB;
    private int score = 0;
    private bool isDragging = false;
    private Vector3 offset;

    void Start()
    {
        // Initialize the save managers
        googlePlaySaveManager = gameObject.AddComponent<GooglePlaySaveManager>();
        firebaseSaveManager = gameObject.AddComponent<FirebaseSaveManager>();

        // Log the initial sign-in state (might be false initially)
        Log("Initial Sign-In Status: " + GooglePlayManager.Instance.IsSignedIn);

        // If already signed in, load immediately
        if (GooglePlayManager.Instance.IsSignedIn)
        {
            StartCoroutine(DelayedLoadData());
        }
        else
        {
            // Wait for sign-in to complete before loading data
            GooglePlayManager.Instance.OnSignInComplete += () =>
            {
                Log("Sign-in event received! Now loading data...");
                StartCoroutine(DelayedLoadData());
            };
        }
    }

    private IEnumerator DelayedLoadData()
    {
        yield return new WaitForSeconds(1f); // Delay for smoother transition

        if (GooglePlayManager.Instance.IsSignedIn) // Now properly updated
        {
            Log("User is authenticated with Google Play Services.");
            googlePlaySaveManager.LoadFromCloud(OnDataLoaded);
        }
        else
        {
            Log("User is not authenticated with Google Play Services.");
        }
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            string clickedUI = IsClickOnUI();

            switch (clickedUI)
            {
                case "UI":
                    Log("Click on save button");
                    SaveGame();
                    break;
                case "ViewLDB":
                    LDB.SetActive(true);
                    Time.timeScale = 0;
                    break;
                case "SocialPanel":
                    Log("Click on social panel");
                    break;
                default:
                    IncreaseScore();
                    break;
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

    string IsClickOnUI()
    {
        EventSystem eventSystem = EventSystem.current;
        PointerEventData eventData = new PointerEventData(eventSystem) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();

        eventSystem.RaycastAll(eventData, results);

        HashSet<string> validTags = new HashSet<string> { "UI", "ViewLDB", "SocialPanel" };

        foreach (var result in results)
        {
            if (validTags.Contains(result.gameObject.tag))
            {
                return result.gameObject.tag;
            }
        }
        return "IncreaseScore";
    }

    void IncreaseScore()
    {
        score++;
        scoreText.text = score.ToString();
    }

    public void SaveGame()
    {
        Log("Saving game...");
        // Load existing data
        DataToSave saveData = LoadSaveDataFromJson();

        // Update data (e.g., add score from this session)
        saveData.score += score;
        saveData.position = movableObject.position;

        // Save to JSON file
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(UnityEngine.Application.persistentDataPath + "/savegame.json", json);

        googlePlaySaveManager.SaveToCloud(score, movableObject.position);

        firebaseSaveManager.SaveToFirebase(Social.localUser.id, googlePlaySaveManager.userId, score, movableObject.position);
    }

    public DataToSave LoadSaveDataFromJson()
    {
        string path = UnityEngine.Application.persistentDataPath + "/savegame.json";

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<DataToSave>(json);
        }

        // Default save data if file doesn't exist
        return new DataToSave(0, Vector3.zero);
    }

    void OnDataLoaded(DataToSave loadedData)
    {
        if (loadedData ==null){
            Log("No saved data found.");
            return;
        }
        // Update the score and movable object position with the loaded data
        score = loadedData.score;
        movableObject.position = new Vector3(loadedData.position.x, loadedData.position.y, loadedData.position.z);
        scoreText.text = score.ToString();

        Log("Game Loaded! Score: " + score);
    }

    public void Log(string message)
    {
        Debug.Log(message);
        if (debugText != null)
        {
            debugText.text = message;
        }
    }
}
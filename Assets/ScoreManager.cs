using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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

        // Load saved data from Google Play Services (if authenticated)
        if (Social.localUser.authenticated)
        {
            googlePlaySaveManager.LoadFromCloud(OnDataLoaded);
        }
        else
        {
            Debug.Log("User is not authenticated with Google Play Services.");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            if (IsClickOnUI().Equals("UI"))
            {
                Log("Click on save button");
                SaveGame();
            }
            else if (IsClickOnUI().Equals("ViewLDB"))
            {
                LDB.SetActive(true);
                Time.timeScale = 0;
            }
            else
            {
                IncreaseScore();
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
        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("UI"))
            {
                return "UI";
            }
            else if (result.gameObject.CompareTag("ViewLDB"))
            {
                return "ViewLDB";
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

        // Save to Google Play Services
        googlePlaySaveManager.SaveToCloud(score, movableObject.position);

        // Save to Firebase
        firebaseSaveManager.SaveToFirebase(Social.localUser.id, googlePlaySaveManager.userId, score, movableObject.position);
    }

    void OnDataLoaded(DataToSave loadedData)
    {
        // Update the score and movable object position with the loaded data
        score = loadedData.score;
        movableObject.position = new Vector3(loadedData.x, loadedData.y, loadedData.z);
        scoreText.text = score.ToString();

        Log("Game Loaded! Score: " + score);
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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MinigameLevelTimer : MinigameLevel
{
    public int timeInSeconds;
    public int targetScore;

    private float timer;
    private bool timeOut = false;

    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public GameObject winPanel;
    public Button winButton;

    private void Start()
    {
        type = LevelType.TIMER;
        winPanel.SetActive(false);
        winButton.onClick.AddListener(OnWinButtonClicked);
    }

    private void OnEnable()
    {
        StartGame(); // Reset game state when reactivated
        UpdateResourceTexts();
    }

    private void Update()
    {
        if (!timeOut)
        {
            timer -= Time.deltaTime;
            timer = Mathf.Max(timer, 0);
            timerText.text = "Time left: " + Mathf.Ceil(timer).ToString();


            if (timer <= 0)
            {
                timeOut = true;
                GameWin();
                ShowWinPanel();
            }
        }
    }

    private void ShowWinPanel()
    {
        winPanel.SetActive(true);
        Debug.Log("Win panel shown");
    }

    private void OnWinButtonClicked()
    {
        Debug.Log("Win button clicked! Collecting resources and deactivating minigame...");

        // Collect resources
        CollectResources();

        // Reset state
        ResetTimer();
        ResourceManager.Instance.ResetResources();
        if (grid != null)
        {
            grid.GameOver();
        }
        currentScore = 0;

        // Hide the win panel
        winPanel.SetActive(false);
        Debug.Log("Win panel hidden");

        // Deactivate the minigame GameObject
        Debug.Log($"Deactivating minigame: {gameObject.name}, Active before: {gameObject.activeSelf}");
        gameObject.SetActive(false);
        Debug.Log($"Minigame active after: {gameObject.activeSelf}");
    }

    private void ResetTimer()
    {
        timer = (float)timeInSeconds;
        timeOut = false;
        if (timerText != null)
        {
            timerText.text = "Time left: " + Mathf.Ceil(timer).ToString();
        }
        Debug.Log($"Timer reset to {timeInSeconds} seconds");
    }

    public void StartGame()
    {
        ResetTimer();
        ResourceManager.Instance.ResetResources();
        if (grid != null)
        {
            grid.ResetGame(); // Reset gameOver and fill grid
            StartCoroutine(grid.Fill()); // Ensure grid is populated
        }
        currentScore = 0;
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        Debug.Log("Started new game");
    }
}
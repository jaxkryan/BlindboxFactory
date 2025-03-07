using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MinigameLevelTimer : MinigameLevel
{
    public int timeInSeconds;
    public int targetScore;

    private float timer; // Use float for time countdown
    private bool timeOut = false;

    [Header("UI Elements")]
    public TextMeshProUGUI timerText; // Assign in Inspector
    public GameObject winPanel; // Assign in Inspector
    public Button winButton; // Assign in Inspector

    private void Start()
    {
        type = LevelType.TIMER;
        timer = (float)timeInSeconds; // Ensure it's treated as a float
        winPanel.SetActive(false); // Hide win panel at start

        winButton.onClick.AddListener(OnWinButtonClicked); // Assign button function

        Debug.Log("Time: " + timeInSeconds + " seconds");
    }

    private void Update()
    {
        if (!timeOut)
        {
            timer -= Time.deltaTime;
            timer = Mathf.Max(timer, 0); // Prevent negative values
            timerText.text = "Time left: " + Mathf.Ceil(timer).ToString(); // Update UI

            Debug.Log("Timer: " + timer); // Debug to check if it is decreasing

            if (timer <= 0)
            {
                timeOut = true; // Set before calling GameWin()
                GameWin();
                ShowWinPanel();
            }
        }
    }

    private void ShowWinPanel()
    {
        winPanel.SetActive(true);
    }

    private void OnWinButtonClicked()
    {
        Debug.Log("Win button clicked! Closing game...");
        Application.Quit(); // Closes the game (for builds)
        // SceneManager.LoadScene("MainMenu"); // Uncomment if returning to menu
    }
}

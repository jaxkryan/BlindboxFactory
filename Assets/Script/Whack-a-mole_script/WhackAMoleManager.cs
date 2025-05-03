using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script.Controller; // For GameController and ResourceController
using Script.Resources; // For Resource enum

public class WhackAMoleManager : MonoBehaviour
{
    public static WhackAMoleManager Instance;

    [SerializeField] private List<Mole> moles;
    [SerializeField] private TMPro.TextMeshProUGUI timeText;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject returnToMinigameButton; // New button for returning to minigame

    private float timeRemaining = 15f;
    private int score = 0;
    private bool playing = false;
    private int maxActiveMoles = 3;
    private HashSet<Mole> activeMoles = new HashSet<Mole>();
    private int targetScore = 15;

    private void Awake()
    {
        Instance = this;

        // Ensure the return button is inactive at the start
        if (returnToMinigameButton != null)
        {
            returnToMinigameButton.SetActive(false);
        }
        else
        {
            //Debug.LogWarning("ReturnToMinigameButton not assigned in Inspector!");
        }
    }

    public void StartGame()
    {
        playButton.SetActive(false);
        if (returnToMinigameButton != null)
        {
            returnToMinigameButton.SetActive(false); // Ensure return button is hidden during gameplay
        }
        timeRemaining = 15f;
        score = 0;
        playing = true;
        scoreText.text = "0";
        activeMoles.Clear();
        StartCoroutine(SpawnMoles());
    }

    private IEnumerator SpawnMoles()
    {
        while (playing && timeRemaining > 0)
        {
            if (activeMoles.Count < maxActiveMoles)
            {
                Mole mole = GetInactiveMole();
                if (mole != null)
                {
                    activeMoles.Add(mole);
                    Mole.MoleType type = (Random.value < 0.25f) ? Mole.MoleType.HardHat : Mole.MoleType.Standard;
                    mole.Show(type);
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private Mole GetInactiveMole()
    {
        List<Mole> inactiveMoles = moles.FindAll(m => !activeMoles.Contains(m));
        if (inactiveMoles.Count > 0)
        {
            return inactiveMoles[Random.Range(0, inactiveMoles.Count)];
        }
        return null;
    }

    private void Update()
    {
        if (playing)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                GameOver();
            }
            int minutes = (int)timeRemaining / 60;
            int seconds = (int)timeRemaining % 60;
            string secondsText = seconds < 10 ? "0" + seconds : seconds.ToString();
            timeText.text = minutes.ToString() + ":" + secondsText;

            if (score == 10)
                maxActiveMoles = 2;
            else if (score == 11)
                maxActiveMoles = 1;
        }
    }

    public void AddScore(Mole.MoleType type)
    {

        AudioManager.Instance.PlaySfx("minigameSfx");
        score++;
        scoreText.text = score.ToString();

        // Nếu là HardHat, cộng thêm 3s
        if (type == Mole.MoleType.HardHat)
        {
            timeRemaining += 3f;
        }

        // Giảm số lượng chuột khi gần đạt mục tiêu
        if (score == 13)
            maxActiveMoles = 2;
        else if (score == 14)
            maxActiveMoles = 1;

        // Kết thúc trò chơi khi đạt điểm mục tiêu
        if (score >= targetScore)
        {
            CompleteGame();
        }
    }

    public void OnMoleHidden(Mole mole)
    {
        activeMoles.Remove(mole);
    }

    public void GameOver()
    {
        playing = false;
        playButton.SetActive(true);
        if (returnToMinigameButton != null)
        {
            returnToMinigameButton.SetActive(true);
        }
        StopAllCoroutines();
    }

    private void CompleteGame()
    {
        playing = false;
        playButton.SetActive(true);
        if (returnToMinigameButton != null)
        {
            returnToMinigameButton.SetActive(true);
        }
        StopAllCoroutines();
        Reward();
    }

    private void Reward()
    {
        if (GameController.Instance != null && GameController.Instance.ResourceController != null)
        {
            // Add 15 Gems
            if (GameController.Instance.ResourceController.TryGetAmount(Resource.Gem, out long currentGems))
            {
                long newGems = currentGems + 15;
                if (!GameController.Instance.ResourceController.TrySetAmount(Resource.Gem, newGems))
                {
                    //Debug.LogWarning("Failed to set Gem amount in ResourceController.");
                }
                else
                {
                    //Debug.Log($"Added 15 Gems. New total: {newGems}");
                }
            }
            else
            {
                //Debug.LogWarning("Failed to get current Gem amount from ResourceController.");
            }

            // List of material resources to update
            Resource[] materials = { Resource.Diamond, Resource.Cloud, Resource.Rainbow, Resource.Gummy, Resource.Ruby, Resource.Star };

            foreach (Resource material in materials)
            {
                if (GameController.Instance.ResourceController.TryGetAmount(material, out long currentAmount))
                {
                    // Calculate 5% to 10% of current amount
                    float randomPercentage = Random.Range(0.05f, 0.10f);
                    long additionalAmount = (long)(currentAmount * randomPercentage);
                    long newAmount = currentAmount + additionalAmount;

                    if (!GameController.Instance.ResourceController.TrySetAmount(material, newAmount))
                    {
                        //Debug.LogWarning($"Failed to set {material} amount in ResourceController.");
                    }
                    else
                    {
                        //Debug.Log($"Added {additionalAmount} {material}. New total: {newAmount}");
                    }
                }
                else
                {
                    //Debug.LogWarning($"Failed to get current {material} amount from ResourceController.");
                }
            }
        }
        else
        {
            //Debug.LogWarning("GameController or ResourceController is not available.");
        }
    }
}
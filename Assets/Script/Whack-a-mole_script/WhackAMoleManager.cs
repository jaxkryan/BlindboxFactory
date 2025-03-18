using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhackAMoleManager : MonoBehaviour
{
    public static WhackAMoleManager Instance;

    [SerializeField] private List<Mole> moles;
    [SerializeField] private TMPro.TextMeshProUGUI timeText;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private GameObject playButton;

    private float timeRemaining = 15f;
    private int score = 0;
    private bool playing = false;
    private int maxActiveMoles = 3;
    private HashSet<Mole> activeMoles = new HashSet<Mole>();
    private int targetScore = 15;

    private void Awake()
    {
        Instance = this;
    }

    public void StartGame()
    {
        playButton.SetActive(false);
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
                EndGame();
            }
            timeText.text = $"{(int)timeRemaining / 60}:{(int)timeRemaining % 60:D2}";

            if (score == 10)
                maxActiveMoles = 2;
            else if (score == 11)
                maxActiveMoles = 1;
        }
    }


    public void AddScore(Mole.MoleType type)
    {
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
            EndGame();
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
        StopAllCoroutines();
    }

    private void EndGame()
    {
        playing = false;
        playButton.SetActive(true);
        StopAllCoroutines();
    }
}
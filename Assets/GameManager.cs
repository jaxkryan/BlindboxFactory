using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public string visitedUserId; // 🔹 Non-static so it's saved properly

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager Initialized. Persistent across scenes.");
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

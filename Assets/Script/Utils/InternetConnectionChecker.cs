using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class InternetConnectionChecker : MonoBehaviour
{
    public static InternetConnectionChecker Instance;
    [SerializeField] private GameObject noInternetPanel; // Panel for no internet notification
    [SerializeField] private TextMeshProUGUI noInternetText; // Text to display "No Internet Connection"
    [SerializeField] private Button retryButton; // Button to retry connection
    [SerializeField] private Button exitButton; // Button to exit the game
    [SerializeField] private string testUrl = "https://www.google.com"; // URL to test connectivity
    [SerializeField] private float checkInterval = 10f; // How often to check internet (in seconds)
    [SerializeField] private float requestTimeout = 5f; // Timeout for HTTP request in seconds

    private bool isConnected = false; // Current connection status
    private bool isChecking = false; // Prevent multiple checks at the same time
    private bool isWaitingForRetry = false; // Track if waiting for user input

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ensure the no internet panel is inactive at the start
        if (noInternetPanel != null)
        {
            noInternetPanel.SetActive(false);
        }

        // Set up button listeners
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryButtonClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }
    }

    private void Start()
    {
        // Start checking internet connection periodically
        StartCoroutine(CheckInternetRoutine());
    }

    private IEnumerator CheckInternetRoutine()
    {
        while (true)
        {
            // Check internet connection if not already waiting for a retry
            if (!isWaitingForRetry)
            {
                yield return StartCoroutine(CheckInternetConnection());

                // If not connected, show the popup and wait for user input
                if (!isConnected)
                {
                    isWaitingForRetry = true;
                    if (noInternetPanel != null)
                    {
                        noInternetPanel.SetActive(true);
                        if (noInternetText != null)
                        {
                            noInternetText.text = "No Internet Connection";
                        }
                    }
                }
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    private IEnumerator CheckInternetConnection()
    {
        if (isChecking) yield break; // Avoid overlapping checks
        isChecking = true;

        // First, check Unity's built-in internet reachability
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            isConnected = false;
            isChecking = false;
            yield break;
        }

        // Perform an HTTP request to verify connectivity
        yield return StartCoroutine(CheckHttpConnection());
    }

    private IEnumerator CheckHttpConnection()
    {
        using (UnityWebRequest request = UnityWebRequest.Head(testUrl))
        {
            request.timeout = Mathf.CeilToInt(requestTimeout);

            // Send the request
            yield return request.SendWebRequest();

            // Check the result
            if (request.result == UnityWebRequest.Result.Success)
            {
                isConnected = true;
            }
            else
            {
                isConnected = false;
                Debug.LogWarning($"Internet check failed: {request.error}");
            }
        }

        isChecking = false;
    }

    public bool IsConnected()
    {
        return isConnected && !isChecking;
    }

    public void ForceCheck()
    {
        StartCoroutine(CheckInternetConnection());
    }

    private void OnRetryButtonClicked()
    {
        StartCoroutine(CheckInternetConnection());
        if (isConnected)
        {
            isWaitingForRetry = false;
            if (noInternetPanel != null)
            {
                noInternetPanel.SetActive(false);
            }
        }
    }

    private void OnExitButtonClicked()
    {
        // Exit the game
        Application.Quit();

        // If running in the Editor, stop playing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}


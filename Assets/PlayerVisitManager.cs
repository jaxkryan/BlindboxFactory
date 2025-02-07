using UnityEngine;
using Firebase.Database;
using TMPro;
using System.Threading;
using UnityEngine.SceneManagement;
public class PlayerVisitManager : MonoBehaviour
{
    public GameObject playerTransform; // Assign in the Inspector
    public TMP_Text playerNameText; // Assign in the Inspector
    private SynchronizationContext mainThreadContext;
    private DatabaseReference dbReference;
    public TMP_Text debugText;

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        mainThreadContext = SynchronizationContext.Current;
        Log("Checking visitedUserId in PlayerVisitScene...");
        string userId = GameManager.Instance.visitedUserId;
        Log($"Retrieved visitedUserId: {userId}");

        if (!string.IsNullOrEmpty(userId))
        {
            LoadPlayerData(userId);
        }
        else
        {
            Log("No userId found in PlayerVisitScene!");
        }
    }

    public void BackBtn()
    {
        SceneManager.LoadScene("PlayScene_Test");
    }
    void Log(string message)
    {
        Debug.Log(message);
        if (debugText != null)
        {
            debugText.text = message;
        }
    }
    void LoadPlayerData(string userId)
    {
        Log("Loading player data from Firebase...");

        dbReference.Child("users").OrderByChild("userId").EqualTo(userId).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Log("Error retrieving data from Firebase.");
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Log($"Firebase Query Completed. Snapshot Count: {snapshot.ChildrenCount}");

                // Ensure the UI update runs on the main thread
                mainThreadContext.Post(_ =>
                {
                    Log("Entered UnityMainThreadDispatcher");

                    if (snapshot.Exists)
                    {
                        foreach (var child in snapshot.Children)
                        {
                            var userIdValue = child.Child("userId").Value;
                            if (userIdValue == null)
                            {
                                Log("Error: userId is null!");
                                continue;
                            }

                            string foundUserId = userIdValue.ToString();
                            float x = float.Parse(child.Child("x")?.Value?.ToString() ?? "0");
                            float y = float.Parse(child.Child("y")?.Value?.ToString() ?? "0");
                            float z = float.Parse(child.Child("z")?.Value?.ToString() ?? "0");

                            Log($"Loaded Player - ID: {foundUserId}, Position: X:{x}, Y:{y}, Z:{z}");

                            // Move the player to the loaded position
                            playerTransform.transform.position = new Vector3(x, y, z);
                            playerNameText.text = $"Visiting: {foundUserId}";
                        }
                    }
                    else
                    {
                        Log("Player not found in Firebase.");
                    }
                }, null);
            }
        });
    }

}

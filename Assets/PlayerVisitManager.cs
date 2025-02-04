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

                if (snapshot.Exists)
                {
                    foreach (var child in snapshot.Children)
                    {
                        string foundUserId = child.Child("userId").Value.ToString();
                        float x = float.Parse(child.Child("x").Value.ToString());
                        float y = float.Parse(child.Child("y").Value.ToString());
                        float z = float.Parse(child.Child("z").Value.ToString());

                        Log($"Loaded Player - ID: {foundUserId}, Position: X:{x}, Y:{y}, Z:{z}");

                        // Move the player to the loaded position
                        mainThreadContext.Post(_ =>
                        {
                            playerTransform.transform.position = new Vector3(x, y, z);
                            playerNameText.text = $"Visiting: {foundUserId}";
                        }, null);
                    }
                }
                else
                {
                    Log("Player not found in Firebase.");
                }
            }
        });
    }
}

using System.Linq;
using Firebase.Database;
using System.Threading;

using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SocialPanelManager : MonoBehaviour
{
    public TMP_InputField searchInput;  // Reference to the search box
    public Button searchButton;

    private SynchronizationContext mainThreadContext;
    public Button closeButton;
    public Transform contentPanel;      // ScrollView Content Panel
    public GameObject playerPrefab;     // Prefab to display player data
    public TMP_Text debugText;
    private DatabaseReference dbReference;

    void Start()
    {
        mainThreadContext = SynchronizationContext.Current;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1;
    }
    // public void Open(){
    //     gameObject.SetActive(true);
    // }
    public void Search()
    {
        SearchPlayer(searchInput.text);
    }
    public void SearchPlayer(string userId)
    {
        Debug.Log("Searching for User ID: " + userId);
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

        mainThreadContext.Post(_ =>
{
    Log("Entered UnityMainThreadDispatcher");

    foreach (Transform child in contentPanel)
    {
        Destroy(child.gameObject);
    }

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

            Log($"Player Found - ID: {foundUserId}, Position: X:{x}, Y:{y}, Z:{z}");

            GameObject newPlayer = Instantiate(playerPrefab, contentPanel);
            newPlayer.transform.Find("PlayerNameText").GetComponent<TMP_Text>().text = $"ID: {foundUserId}";
        }
    }
    else
    {
        Log("Player not found.");
    }
}, null);
    }
});
    }

    void Log(string message)
    {
        Debug.Log(message);
        if (debugText != null)
        {
            debugText.text = message;
        }
    }
    // private string GetShortUserId(string googleId)
    // {
    //     string digits = new string(googleId.Where(char.IsDigit).ToArray());
    //     return digits.Length > 9 ? digits.Substring(0, 9) : digits;
    // }
}

using Firebase.Database;
using UnityEngine;

public class FirebaseSaveManager : MonoBehaviour
{
    private DatabaseReference dbRef;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void SaveToFirebase(string googleId, string userId, int score, Vector3 position)
    {
        if (googleId == null || userId == null)
        {
            Debug.Log("Error: Not authenticated with Firebase when saving!");
            return;
        }
        DataToSave data = new DataToSave
        {
            googleId = googleId,
            userId = userId,
            score = score,
            x = position.x,
            y = position.y,
            z = position.z
        };
        string json = JsonUtility.ToJson(data);
        dbRef.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("Error: Failed to save data to Firebase.");
            }
            else if (task.IsCompleted)
            {
                Debug.Log("Data saved to Firebase.");
            }
        });
    }
}
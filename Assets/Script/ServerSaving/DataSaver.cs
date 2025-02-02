using UnityEngine;
using System;
using Firebase.Database;

[Serializable] public class dataToSave{
    public string userName;
    public float x;

    public float y;

    public float z;

    public int score;
}
public class DataSaver : MonoBehaviour
{
    public dataToSave dts;
    public string userId;
    DatabaseReference dbRef;

    private void Awake(){
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void SaveData(){
        string json = JsonUtility.ToJson(dts);
        dbRef.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWith(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to save data");
            }
            else if (task.IsCompleted)
            {
                Debug.Log("Data saved successfully");
            }
        });
    }

    public void LoadData(){

    }
}

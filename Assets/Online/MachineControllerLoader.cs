using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class MachineControllerLoader : MonoBehaviour
{
    private DatabaseReference _databaseReference;

    public string userId = "local_user";

    OnlineMachineBuilder onlineMachineBuilder = new();

    void Start()
    {
        LoadMachineControllerData(userId);
    }

    void LoadMachineControllerData(string userId)
    {
        string path = $"users/{userId}/MachineController";
        Debug.Log($"Attempting to load data from path: {path}");

        FirebaseDatabase.DefaultInstance.GetReference(path).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log($"Data retrieval completed. Snapshot: {snapshot}");

                if (snapshot.Exists && snapshot.Value != null)
                {
                    string jsonString = snapshot.Value.ToString();
                    Debug.Log("Raw JSON String:");
                    Debug.Log(jsonString);

                    try
                    {
                        // Deserialize the raw JSON into SaveData
                        SaveData data = JsonConvert.DeserializeObject<SaveData>(jsonString);
                        onlineMachineBuilder.BuildFromFirebaseData(data.Machines);
                        if (data != null && data.Machines != null)
                        {
                            foreach (var machine in data.Machines)
                            {
                                Debug.Log($"Machine PrefabName: {machine.PrefabName}");
                                Debug.Log($"Machine Position: x={machine.Position.x}, y={machine.Position.y}, z={machine.Position.z}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Deserialized data is null or Machines list is empty!");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Failed to parse JSON: " + e.Message);
                    }
                }
                else
                {
                    Debug.LogWarning("Snapshot does not exist or value is null.");
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve data: " + task.Exception);
            }
        });


    }

    [System.Serializable]
    public class SaveData
    {
        public List<MachineBaseData> Machines;
    }

    [System.Serializable]
    public class MachineBaseData
    {
        public string PrefabName;
        public PositionData Position;
    }

    [System.Serializable]
    public class PositionData
    {
        public float x;
        public float y;
        public float z;
    }

}

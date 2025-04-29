using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

public class MachineControllerLoader : MonoBehaviour
{
    private DatabaseReference _databaseReference;

    public string userId = "local_user";

    public OnlineMachineBuilder onlineMachineBuilder;

    void Start()
    {
        if (!string.IsNullOrEmpty(UserIdHolder.UserId))
        {
            userId = UserIdHolder.UserId;
        }

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
                Debug.Log($"Data retrieval completed. Snapshot exists: {snapshot.Exists}");

                if (snapshot.Exists && snapshot.Value != null)
                {
                    string jsonString = snapshot.GetRawJsonValue();
                    Debug.Log("Raw JSON First 100 chars: " + jsonString.Substring(0, Mathf.Min(100, jsonString.Length)));

                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        try
                        {
                            // Handle double-serialized JSON
                            string correctedJson = jsonString;
                            if (jsonString.StartsWith("\"") && jsonString.EndsWith("\""))
                            {
                                correctedJson = JsonConvert.DeserializeObject<string>(jsonString);
                                Debug.Log("Corrected JSON First 100 chars: " + correctedJson.Substring(0, Mathf.Min(100, correctedJson.Length)));
                            }

                            // Parse JSON into a JObject
                            JObject jsonObject = JObject.Parse(correctedJson);

                            // Create SaveData instance
                            SaveData saveData = new SaveData
                            {
                                Machines = new List<MachineBaseData>()
                            };

                            // Extract Machines array
                            JArray machinesArray = jsonObject["Machines"]?["$values"] as JArray;
                            if (machinesArray != null)
                            {
                                foreach (JObject machineObj in machinesArray)
                                {
                                    MachineBaseData machineData = new MachineBaseData
                                    {
                                        PrefabName = machineObj["PrefabName"]?.Value<string>(),
                                        Position = new PositionData
                                        {
                                            x = machineObj["Position"]?["x"]?.Value<float>() ?? 0f,
                                            y = machineObj["Position"]?["y"]?.Value<float>() ?? 0f,
                                            z = machineObj["Position"]?["z"]?.Value<float>() ?? 0f
                                        }
                                    };
                                    saveData.Machines.Add(machineData);
                                }
                            }
                            else
                            {
                                Debug.LogWarning("Machines array not found or empty in JSON.");
                            }

                            // Pass data to builder
                            onlineMachineBuilder.BuildFromFirebaseData(saveData.Machines);

                            // Log deserialized data
                            if (saveData.Machines != null)
                            {
                                foreach (var machine in saveData.Machines)
                                {
                                    Debug.Log($"Machine PrefabName: {machine.PrefabName}");
                                    Debug.Log($"Machine Position: x={machine.Position.x}, y={machine.Position.y}, z={machine.Position.z}");
                                }
                            }
                            else
                            {
                                Debug.LogWarning("Machines list is null or empty!");
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"Failed to parse JSON: {e.Message}\nStackTrace: {e.StackTrace}");
                            if (e.InnerException != null)
                            {
                                Debug.LogError($"Inner Exception: {e.InnerException.Message}\nInner StackTrace: {e.InnerException.StackTrace}");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Raw JSON is empty or null.");
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
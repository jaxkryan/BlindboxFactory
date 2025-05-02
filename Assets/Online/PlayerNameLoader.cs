using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameLoader : MonoBehaviour
{
    public TMP_Text factoryText; // Assign your Text UI here
    public string userId;

    void Start()
    {
        if (!string.IsNullOrEmpty(UserHolder.UserId))
        {
            userId = UserHolder.UserId;
        }

         factoryText.text = $"{UserHolder.UserName}'s factory";
    }
}

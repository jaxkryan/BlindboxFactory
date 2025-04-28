using System;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public string Id;
    public string PlayerName;
    public DateTime FirstLogin;
    public DateTime LastLogin;

    public PlayerData(string userId)
    {
        Id = userId;
        PlayerName = Social.localUser.userName == null ? Social.localUser.userName : "local_user";
        FirstLogin = DateTime.Now;
        LastLogin = DateTime.Now;
    }

    public PlayerData()
    {
    }
}
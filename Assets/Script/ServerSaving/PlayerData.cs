using System;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public string Id;
    public DateTime FirstLogin;
    public DateTime LastLogin;

    public PlayerData(string userId)
    {
        Id = userId;
        FirstLogin = DateTime.Now;
        LastLogin = DateTime.Now;
    }

    public PlayerData()
    {
    }
}
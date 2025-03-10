using UnityEngine;

using System;

[Serializable]
public class DataToSave
{
    public string googleId;
    public string userId;
    public Vector3 position;
    public int score;

    public DataToSave(string googleId, string userId, int score, Vector3 position)
    {
        this.googleId = googleId;
        this.userId = userId;
        this.score = score;
        this.position = position;
    }
    public DataToSave()
    {

    }
    public DataToSave(int score, Vector3 position)
    {
        this.score = score;
        this.position = position;
    }
}
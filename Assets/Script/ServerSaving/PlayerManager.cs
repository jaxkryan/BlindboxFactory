using UnityEngine;
using LootLocker.Requests;
using System;
using Random = UnityEngine.Random;
public class PlayerManager : MonoBehaviour
{
    public int playerId { get; private set; }
    public String playerIdentifier { get; private set; }
    public bool playerLoggedIn;

    public void StartSession()
    {
        playerIdentifier = SystemInfo.deviceUniqueIdentifier;

        LootLockerSDKManager.StartGoogleSession(playerIdentifier, (response) =>
        {
            if (response.success)
            {
                playerId = response.player_id;
                playerLoggedIn = true;

                String playerName = "Player #" + Random.Range(0, 1000);
                LootLockerSDKManager.SetPlayerName(playerName, response => { });
            }
        }
        );
    }
}

using System;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public class InitializeUGS : MonoBehaviour
{

    public string environment = "newenvironment";

    async void Start()
    {
        try
        {
            var options = new InitializationOptions()
                .SetEnvironmentName(environment);

            await UnityServices.InitializeAsync(options);
        }
        catch (System.Exception exception)
        {
            // An error occurred during initialization.
        }
    }
}
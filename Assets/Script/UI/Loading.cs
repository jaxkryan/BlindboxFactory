using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public Slider progressBar;

    private void Start()
    {
        // Turn off audio at the start of loading
        AudioManager.Instance.PlayMusic(""); // Stop music
        AudioManager.Instance.PlaySfx(""); // Stop SFX

        StartCoroutine(LoadGameAsync());
    }

    private IEnumerator LoadGameAsync()
    {
        // Step 1: Check for internet connection using InternetConnectionChecker
        if (InternetConnectionChecker.Instance != null)
        {
            // Force an initial check
            InternetConnectionChecker.Instance.ForceCheck();

            // Wait until there's an internet connection
            while (!InternetConnectionChecker.Instance.IsConnected())
            {
                yield return null; // Wait for user to retry or exit via InternetConnectionChecker
            }
        }

        // Step 2: Fake loading progress
        float fakeProgress = 0f;
        while (fakeProgress < 0.9f)
        {
            fakeProgress += Time.deltaTime * 0.5f; // Adjust this for speed
            if (progressBar) progressBar.value = fakeProgress;
            yield return null;
        }

        // Step 3: Simulate final loading
        if (progressBar) progressBar.value = 1f;
        yield return new WaitForSeconds(0.5f);

        // Step 4: Deactivate the loading GameObject
        gameObject.SetActive(false);

        // Turn audio back on after loading finishes
        AudioManager.Instance.PlayMusic("Adventure");
    }
}
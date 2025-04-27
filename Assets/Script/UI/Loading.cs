using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public Slider progressBar;

    [SerializeField] private string sceneToLoad = "MainScreen";

    void Start()
    {
        StartCoroutine(LoadMainSceneAsync());
    }

    IEnumerator LoadMainSceneAsync()
    {

        float fakeProgress = 0f;

        // Step 1: Fake progress up to 90%
        while (fakeProgress < 0.9f)
        {
            fakeProgress += Time.deltaTime * 0.5f; // Adjust this for speed
            if (progressBar) progressBar.value = fakeProgress;
            yield return null;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;


        while (!operation.isDone)
        {
            //float progress = Mathf.Clamp01(operation.progress / 0.9f);
            //if (progressBar) progressBar.value = progress;

            if (operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.5f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}

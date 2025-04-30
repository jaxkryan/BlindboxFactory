using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainScreenButton : MonoBehaviour
{

    public void OnBackButtonClick()
    {
        SceneManager.LoadScene("MainScreen");
    }
}

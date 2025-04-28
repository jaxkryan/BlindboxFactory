using UnityEngine;
using UnityEngine.SceneManagement;

public class VisitButton : MonoBehaviour
{
    public string userIdToLoad; 
    public string sceneToLoad;  

    public void OnButtonClick()
    {
        UserIdHolder.UserId = userIdToLoad; 
        SceneManager.LoadScene(sceneToLoad);  
    }
}

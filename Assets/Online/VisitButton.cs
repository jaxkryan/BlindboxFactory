using UnityEngine;
using UnityEngine.SceneManagement;

public class VisitButton : MonoBehaviour
{
    public string userIdToLoad; 
    public string userNameToLoad;
    public string sceneToLoad;  

    public void OnButtonClick()
    {
        UserHolder.UserId = userIdToLoad; 
        UserHolder.UserName = userNameToLoad;
        SceneManager.LoadScene(sceneToLoad);  
    }
}

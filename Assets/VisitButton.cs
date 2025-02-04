using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VisitButton : MonoBehaviour
{
    private string userId;

    public void SetUserId(string id)
    {
        userId = id;
    }

    public void VisitPlayer()
    {
        GameManager.Instance.visitedUserId = userId;
        Debug.Log($"Visiting Player: {userId}");
        SceneManager.LoadScene("PlayerVisit");
    }
}

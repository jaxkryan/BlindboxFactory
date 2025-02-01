using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TMP_Text scoreText;
    private int score = 0;
    public float rotationSpeed = 100f; // Speed of rotation
    public GameObject rotatingObject;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) //if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            IncreaseScore();
        }

        if (rotatingObject != null)
        {
            rotatingObject.transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
    }

    void IncreaseScore()
    {
        score++;
        scoreText.text = score.ToString();
    }
}

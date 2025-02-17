using System;
using System.Collections;
using UnityEngine;

public class MoveablePiece : MonoBehaviour
{

    private GamePiece piece;
    private IEnumerator moveCorountine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        piece = GetComponent<GamePiece>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Move(int newX, int newY,float time)
    {
        if (moveCorountine != null) {
            StopCoroutine(moveCorountine); 
        }
        moveCorountine = MoveCorountine(newX, newY,time);
        StartCoroutine(moveCorountine);
    }

    private IEnumerator MoveCorountine(int newX, int newY, float time)
    {
        piece.X = newX;
        piece.Y = newY;

        Vector3 startPos = transform.position; 
        Vector3 endPos = piece.GridRef.GetWorldPosition(newX,newY);

        for(float t = 0; t <= 1 * time; t += Time.deltaTime)
        {
            piece.transform.position = Vector3.Lerp(startPos, endPos, t/time);
            yield return 0;
        }

        piece.transform.position = endPos;
    }
}

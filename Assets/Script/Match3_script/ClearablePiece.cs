using System.Collections;
using UnityEngine;

public class ClearablePiece : MonoBehaviour
{
    public AnimationClip clearAnimation;

    private bool isBeingCleared = false;

    public bool IsBeingCleared
    {
        get { return isBeingCleared; }
    }

    protected GamePiece piece;

    private void Awake()
    {
        piece = GetComponent<GamePiece>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Clear()
    {
        piece.GridRef.level.OnPieceClear(piece);
        isBeingCleared = true;
        StartCoroutine(ClearCorountine());

    }

    private IEnumerator ClearCorountine()
    {
        Animator animator = GetComponent<Animator>();

        if (animator)
        {
            animator.Play(clearAnimation.name);

            yield return new WaitForSeconds(clearAnimation.length);

            Destroy(gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Mole : MonoBehaviour
{
    [Header("Graphics")]
    [SerializeField] private Sprite mole;
    [SerializeField] private Sprite moleHardHat;
    [SerializeField] private Sprite moleHatBroken;
    [SerializeField] private Sprite moleHit;
    [SerializeField] private Sprite moleHatHit;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D;
    private Vector3 hiddenPosition;
    private Vector3 visiblePosition;
    private bool hittable = true;
    private int lives;
    public enum MoleType { Standard, HardHat, Bomb };
    private MoleType moleType;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        hiddenPosition = transform.localPosition;
        visiblePosition = new Vector3(hiddenPosition.x, hiddenPosition.y + 2.35f, hiddenPosition.z);
    }

    public void Show(MoleType type)
    {
        moleType = type;
        hittable = true;
        switch (moleType)
        {
            case MoleType.HardHat:
                spriteRenderer.sprite = moleHardHat;
                lives = 2;
                break;
            case MoleType.Standard:
                spriteRenderer.sprite = mole;
                lives = 1;
                break;
            case MoleType.Bomb:
                spriteRenderer.sprite = mole;
                break;
        }
        boxCollider2D.enabled = true;
        transform.DOLocalMove(visiblePosition, 0.3f);
    }

    public void Hide()
    {
        boxCollider2D.enabled = false;
        transform.DOLocalMove(hiddenPosition, 0.3f).OnComplete(() => hittable = false);
    }

    private void OnMouseDown()
    {
        if (hittable)
        {
            if (moleType == MoleType.HardHat)
            {
                if (lives == 2)
                {
                    spriteRenderer.sprite = moleHatBroken;
                    lives--;
                    return;
                }
                spriteRenderer.sprite = moleHatHit;
            }
            else if (moleType == MoleType.Bomb)
            {
                WhackAMoleManager.Instance.GameOver();
                return;
            }
            else
            {
                spriteRenderer.sprite = moleHit;
            }

            hittable = false;
            WhackAMoleManager.Instance.AddScore(moleType);
            StartCoroutine(DisappearDelay());
        }
    }

    private IEnumerator DisappearDelay()
    {
        yield return new WaitForSeconds(0.5f);
        Hide();
        yield return new WaitForSeconds(0.5f);
        WhackAMoleManager.Instance.OnMoleHidden(this);
    }
}
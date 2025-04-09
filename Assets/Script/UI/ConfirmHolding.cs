using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConfirmHolding : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] public float holdTime = 3f;
    private float timer = 0f;
    private bool isHolding = false;

    [SerializeField] public Image progressCircle;
    [SerializeField] public Button targetButton;
    void Start()
    {
        if (targetButton != null)
        {
            targetButton.interactable = false;
        }
    }
    void Update()
    {
        if (isHolding)
        {
            timer += Time.deltaTime;
            progressCircle.fillAmount = (holdTime - timer) / holdTime;

            if (timer >= holdTime)
            {
                isHolding = false;
                progressCircle.fillAmount = 0f;
                OnButtonHoldComplete();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        timer = 0f;
        progressCircle.fillAmount = 1f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (timer < holdTime)
        {
            ResetHold();
        }
    }

    private void ResetHold()
    {
        isHolding = false;
        timer = 0f;
        progressCircle.fillAmount = 0f;
    }

    private void OnButtonHoldComplete()
    {
        targetButton.onClick.Invoke();
    }
}

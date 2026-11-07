using UnityEngine;
using UnityEngine.EventSystems;

public class UISlideDrawer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slide Settings")]
    [SerializeField] private Vector2 slideOffset = new(300f, 0f); 
    [SerializeField] private float slideSpeed = 12f;    

    private RectTransform rectTransform;
    private Vector2 shownPosition;
    private Vector2 hiddenPosition;
    private bool isHovered = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        shownPosition = rectTransform.anchoredPosition;
        hiddenPosition = shownPosition + slideOffset;
        rectTransform.anchoredPosition = hiddenPosition;
    }

    private void Update()
    {
        Vector2 targetPosition = isHovered ? shownPosition : hiddenPosition;
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, Time.unscaledDeltaTime * slideSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData) => isHovered = true;
    public void OnPointerExit(PointerEventData eventData) => isHovered = false;
}
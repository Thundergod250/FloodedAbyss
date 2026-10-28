using UnityEngine;
using UnityEngine.UI;

public class UIGeneralScrollPanelReset : MonoBehaviour
{
    [Header("Scroll View Settings")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentContainer;

    protected virtual void OnEnable() => ResetScrollToTop();

    public void ResetScrollToTop()
    {
        if (scrollRect == null || contentContainer == null) return;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);
        scrollRect.verticalNormalizedPosition = 1f;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResourceTooltipTrigger : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    public enum BindingMode { Manual, AutoByIcon }

    [Header("Binding")]
    [Tooltip("Manual = assign the ResourceDataSO below. " +
             "AutoByIcon = match the icon sprite against loaded ResourceDataSO assets.")]
    [SerializeField] private BindingMode bindingMode = BindingMode.AutoByIcon;

    [Tooltip("Used in Manual mode. Also populated automatically in AutoByIcon mode.")]
    [SerializeField] private ResourceDataSO resourceData;

    [Header("Auto Bind")]
    [Tooltip("Leave empty! It will automatically find the 'Resource Image' child.")]
    [SerializeField] private Image iconImage;

    private void Awake()
    {
        // 1. Try to find the exact child named "Resource Image" based on your prefab structure
        if (iconImage == null)
        {
            Transform iconTransform = transform.Find("Resource Image");
            if (iconTransform != null)
            {
                iconImage = iconTransform.GetComponent<Image>();
            }
        }

        // 2. Fallback: Look for any Image on a CHILD (skipping the root background)
        if (iconImage == null)
        {
            var images = GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                // Skip the root because that's the dark background box
                if (img.gameObject != gameObject)
                {
                    iconImage = img;
                    break;
                }
            }
        }

        // 3. Ensure the icon can detect the mouse
        if (iconImage != null && !iconImage.raycastTarget)
            iconImage.raycastTarget = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (resourceData == null && bindingMode == BindingMode.AutoByIcon)
            TryAutoBind();

        if (resourceData == null || TooltipUI.Instance == null) return;

        // Pass the pointer position directly from the event data
        TooltipUI.Instance.Show(resourceData.ResourceName, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null) TooltipUI.Instance.Hide();
    }

    /// <summary>Call this from code if you prefer explicit binding.</summary>
    public void SetResource(ResourceDataSO data) => resourceData = data;

    private void TryAutoBind()
    {
        if (bindingMode != BindingMode.AutoByIcon) return;
        if (resourceData != null) return;
        if (iconImage == null || iconImage.sprite == null) return;

        var all = Resources.FindObjectsOfTypeAll<ResourceDataSO>();
        foreach (var r in all)
        {
            if (r != null && r.Icon == iconImage.sprite)
            {
                resourceData = r;
                return;
            }
        }
    }
}
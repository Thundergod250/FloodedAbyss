using UnityEngine;
using UnityEngine.EventSystems;

public class MouseHoverHighlight : MonoBehaviour
{
    [SerializeField] private MeshRenderer highlightRenderer;

    private void Awake()
    {
        if (highlightRenderer != null) 
            highlightRenderer.enabled = false;
    }

    private void OnMouseEnter()
    {
        if (highlightRenderer != null) 
            highlightRenderer.enabled = true;
    }

    private void OnMouseExit()
    {
        if (highlightRenderer != null) 
            highlightRenderer.enabled = false;
    }
}
using System.Collections.Generic;
using UnityEngine;

public class HoverHighlight : MonoBehaviour
{
    [Tooltip("The material that uses the Outline shader.")]
    [SerializeField] private Material outlineMaterial;

    [Tooltip("How much larger the outline shell should be scaled (e.g., 1.03 to 1.05).")]
    [SerializeField] private float outlineScale = 1.03f;

    [Tooltip("Should the outline match the object's original color?")]
    [SerializeField] private bool matchObjectColor = false;
    [SerializeField] private Color fallbackOutlineColor = Color.white;

    private List<GameObject> outlineContainers = new();
    private Material uniqueOutlineMaterial;

    private void Awake() => BuildOutlineShells();

    private void BuildOutlineShells()
    {
        if (outlineMaterial == null) return;

        // Find ALL mesh filters in self and children
        MeshFilter[] targetMeshFilters = GetComponentsInChildren<MeshFilter>();
        if (targetMeshFilters.Length == 0)
        {
            Debug.LogWarning($"[HoverHighlight] No MeshFilters found on {gameObject.name} or its children!", this);
            return;
        }

        uniqueOutlineMaterial = new Material(outlineMaterial);

        foreach (var targetMf in targetMeshFilters)
        {
            if (targetMf.transform.name.Contains("Outline")) continue;
            
            if (matchObjectColor)
            {
                Renderer sourceRenderer = targetMf.GetComponent<Renderer>();
                if (sourceRenderer != null && sourceRenderer.sharedMaterial.HasProperty("_Color"))
                {
                    Color baseColor = sourceRenderer.sharedMaterial.GetColor("_Color");
                    uniqueOutlineMaterial.SetColor("_OutlineColor", baseColor);
                }
                else
                    uniqueOutlineMaterial.SetColor("_OutlineColor", fallbackOutlineColor);
            }
            
            GameObject outlineContainer = new GameObject("OutlineShell");
            outlineContainer.transform.SetParent(targetMf.transform, false);
            outlineContainer.transform.localPosition = Vector3.zero;
            outlineContainer.transform.localRotation = Quaternion.identity;
            outlineContainer.transform.localScale = Vector3.one * outlineScale;
            
            GameObject clone = new GameObject("OutlineMesh");
            clone.transform.SetParent(outlineContainer.transform, false);
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localRotation = Quaternion.identity;
            clone.transform.localScale = Vector3.one;

            MeshFilter cloneMf = clone.AddComponent<MeshFilter>();
            cloneMf.sharedMesh = targetMf.sharedMesh;

            MeshRenderer cloneMr = clone.AddComponent<MeshRenderer>();
            Material[] outlineMats = new Material[targetMf.sharedMesh.subMeshCount];
            for (int i = 0; i < outlineMats.Length; i++) 
                outlineMats[i] = uniqueOutlineMaterial;
            cloneMr.sharedMaterials = outlineMats;

            outlineContainer.SetActive(false);
            outlineContainers.Add(outlineContainer);
        }
    }

    private void OnMouseEnter()
    {
        foreach (var container in outlineContainers)
        {
            if (container != null) 
                container.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        foreach (var container in outlineContainers)
        {
            if (container != null) 
                container.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (uniqueOutlineMaterial != null) 
            Destroy(uniqueOutlineMaterial);
    }
}
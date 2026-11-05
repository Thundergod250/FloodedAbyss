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

    private GameObject outlineContainer;
    private Material uniqueOutlineMaterial;

    private void Awake() => BuildOutlineShell();

    private void BuildOutlineShell()
    {
        if (outlineMaterial == null) return;
        MeshFilter targetMeshFilter = GetComponentInChildren<MeshFilter>();
        if (targetMeshFilter == null)
        {
            Debug.LogWarning($"[HoverHighlight] No MeshFilter found on {gameObject.name} or its children!", this);
            return;
        }
        uniqueOutlineMaterial = new Material(outlineMaterial);
        if (matchObjectColor)
        {
            Renderer firstRenderer = targetMeshFilter.GetComponent<Renderer>();
            if (firstRenderer != null && firstRenderer.sharedMaterial.HasProperty("_Color"))
            {
                Color baseColor = firstRenderer.sharedMaterial.GetColor("_Color");
                uniqueOutlineMaterial.SetColor("_OutlineColor", baseColor);
            }
            else
                uniqueOutlineMaterial.SetColor("_OutlineColor", fallbackOutlineColor);
        }
        
        outlineContainer = new GameObject("OutlineShell");
        outlineContainer.transform.SetParent(targetMeshFilter.transform, false);
        outlineContainer.transform.localPosition = Vector3.zero;
        outlineContainer.transform.localRotation = Quaternion.identity;
        outlineContainer.transform.localScale = Vector3.one * outlineScale;
        
        GameObject clone = new GameObject("OutlineMesh");
        clone.transform.SetParent(outlineContainer.transform, false);
        clone.transform.localPosition = Vector3.zero;
        clone.transform.localRotation = Quaternion.identity;
        clone.transform.localScale = Vector3.one;
        MeshFilter cloneMf = clone.AddComponent<MeshFilter>();
        cloneMf.sharedMesh = targetMeshFilter.sharedMesh;
        MeshRenderer cloneMr = clone.AddComponent<MeshRenderer>();
        Material[] outlineMats = new Material[targetMeshFilter.sharedMesh.subMeshCount];
        for (int i = 0; i < outlineMats.Length; i++) 
            outlineMats[i] = uniqueOutlineMaterial;
        cloneMr.sharedMaterials = outlineMats;
        outlineContainer.SetActive(false);
    }

    private void OnMouseEnter()
    {
        if (outlineContainer != null) 
            outlineContainer.SetActive(true);
    }

    private void OnMouseExit()
    {
        if (outlineContainer != null) 
            outlineContainer.SetActive(false);
    }

    private void OnDestroy()
    {
        if (uniqueOutlineMaterial != null) 
            Destroy(uniqueOutlineMaterial);
    }
}
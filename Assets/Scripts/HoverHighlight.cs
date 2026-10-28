using System.Collections.Generic;
using UnityEngine;

public class HoverHighlight : MonoBehaviour
{
    [Tooltip("The material that uses the Outline shader.")]
    [SerializeField] private Material outlineMaterial;

    [Tooltip("Should the outline match the object's original color? If false, it uses the color from the material.")]
    [SerializeField] private bool matchObjectColor = false;

    private Renderer[] childRenderers;
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    private void Awake()
    {
        // Grab all meshes in this object (including children)
        childRenderers = GetComponentsInChildren<Renderer>();

        foreach (var renderer in childRenderers)
        {
            // Save the original materials so we can restore them later
            originalMaterials[renderer] = renderer.materials;
        }
    }

    private void OnMouseEnter()
    {
        if (outlineMaterial == null) return;

        foreach (var renderer in childRenderers)
        {
            Material[] original = originalMaterials[renderer];
            Material[] highlighted = new Material[original.Length + 1];

            // Copy original materials
            for (int i = 0; i < original.Length; i++)
            {
                highlighted[i] = original[i];
            }

            // Add the outline material to the end of the array
            highlighted[original.Length] = outlineMaterial;

            // If requested, match the outline color to the main object's color
            if (matchObjectColor && original.Length > 0 && original[0].HasProperty("_Color"))
            {
                outlineMaterial.SetColor("_OutlineColor", original[0].GetColor("_Color"));
            }

            renderer.materials = highlighted;
        }
    }

    private void OnMouseExit()
    {
        foreach (var renderer in childRenderers)
        {
            // Restore original materials
            renderer.materials = originalMaterials[renderer];
        }
    }
}
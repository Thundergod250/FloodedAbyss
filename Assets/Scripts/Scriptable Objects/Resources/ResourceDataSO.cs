using UnityEngine;

[CreateAssetMenu(fileName = "NewResource", menuName = "Resource")]
public class ResourceDataSO : ScriptableObject
{
    [field: SerializeField] public string ResourceName { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public int StartingAmount { get; private set; } = 0;
}
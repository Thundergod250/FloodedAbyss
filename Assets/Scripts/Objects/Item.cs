using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public abstract void Activate(); 
    public virtual void GainPlayerReference(PlayerResources resourceScript)
    {
    }
}
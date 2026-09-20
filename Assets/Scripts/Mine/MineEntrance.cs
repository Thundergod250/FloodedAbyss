using UnityEngine;

public class MineEntrance : MonoBehaviour
{
    [Header("Mine Manager")]
    public MineManager mineManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            EnterMine();
        }
    }

    public void EnterMine()
    {
        Debug.Log("Player entered the mine!");

        if (mineManager != null)
        {
            mineManager.EnterMine();
        }
        else
        {
            Debug.LogWarning("MineManager is not assigned!");
        }
    }

    public void CancelEntrance()
    {
        // Confirmation UI
    }
}
using UnityEngine;

public class DungeonExit : MonoBehaviour
{
    [Header("Mine Manager")]
    public MineManager mineManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            LeaveDungeon();
        }
    }

    public void LeaveDungeon()
    {
        Debug.Log("Player exited the Mine!");

        if (mineManager != null)
        {
            mineManager.LeaveMine();
        }
        else
        {
            Debug.LogWarning("MineManager is not assigned!");
        }
    }

    public void CancelExit()
    {
        // Confirmation UI
    }
}
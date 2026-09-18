using UnityEngine;

public class DungeonExit : MonoBehaviour
{
    public GameObject confirmationUI;

    private void Start()
    {
        confirmationUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            confirmationUI.SetActive(true);
        }
    }

    public void LeaveDungeon()
    {
        Debug.Log("Leaving dungeon...");
    }

    public void CancelExit()
    {
        confirmationUI.SetActive(false);
    }
}
using UnityEngine;

public class MineEntrance : MonoBehaviour
{
    public GameObject confirmationUI;

    private void Start()
    {
        confirmationUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerMovement>() != null)
        {
            confirmationUI.SetActive(true);
        }
    }

    public void EnterMine()
    {
        Debug.Log("Entering the mine...");

    }

    public void CancelEntrance()
    {
        confirmationUI.SetActive(false);
    }
}
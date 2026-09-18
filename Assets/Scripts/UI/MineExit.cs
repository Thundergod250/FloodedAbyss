using UnityEngine;

public class MineExit : MonoBehaviour
{
    //public GameObject confirmationUI;

    private void Start()
    {
        //confirmationUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            //confirmationUI.SetActive(true);
            LeaveMine();
        }
    }

    public void LeaveMine()
    {
        Debug.Log("Leaving Mine...");
    }

    public void CancelExit()
    {
        //confirmationUI.SetActive(false);
    }
}
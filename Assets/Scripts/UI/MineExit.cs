using UnityEngine;

public class MineExit : MonoBehaviour
{
    //public GameObject confirmationUI;
    public MineManager mineManager;

    private void Start()
    {
        //confirmationUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            //confirmationUI.SetActive(true);
            mineManager.LeaveMine();
        }
    }

    public void LeaveMine()
    {
        mineManager.LeaveMine();
        Debug.Log("Leaving Mine...");
    }

    public void CancelExit()
    {
        //confirmationUI.SetActive(false);
    }
}
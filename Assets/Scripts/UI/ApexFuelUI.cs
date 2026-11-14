using TMPro;
using UnityEngine;
using System.Collections;

public class ApexFuelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fuelText;
    [SerializeField] private float updateInterval = 2f;

    private Coroutine updateRoutine;

    // Update is called once per frame
    private void OnEnable()
    {
        if (updateRoutine == null)
            updateRoutine = StartCoroutine(UpdateUIRoutine());
    }

    private void OnDisable()
    {
        if (updateRoutine != null)
        {
            StopCoroutine(updateRoutine);
            updateRoutine = null;
        }
    }
    private IEnumerator UpdateUIRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(updateInterval);

        while (true)
        {
            if (fuelText != null)
            {
                fuelText.text = GameManager.Instance.ApexPump.CurrentFuel.ToString();
            }

            yield return wait;
        }
    }
}

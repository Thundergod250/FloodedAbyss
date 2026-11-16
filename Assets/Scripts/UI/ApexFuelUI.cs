using TMPro;
using UnityEngine;
using System.Collections;

public class ApexFuelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fuelText;
    [SerializeField] private float updateInterval = 2f;

    private Coroutine updateRoutine;

    private void OnEnable()
    {
        UpdateFuelUI();

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
        WaitForSeconds wait = new WaitForSeconds(
            Mathf.Max(0.1f, updateInterval)
        );

        while (true)
        {
            UpdateFuelUI();
            yield return wait;
        }
    }

    private void UpdateFuelUI()
    {
        if (fuelText == null ||
            GameManager.Instance == null ||
            GameManager.Instance.ApexPump == null)
            return;

        fuelText.text = GameManager.Instance.ApexPump.CurrentFuel.ToString();
    }
}

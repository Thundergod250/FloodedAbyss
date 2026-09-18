using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Stats UI Bar")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider staminaBar;
    [SerializeField] private Slider oxygenBar;

    private GameObject playerReference;
    private Health playerH;
    private PlayerStamina playerSt;
    private PlayerOxygen playerOxygen;

    private void Start()
    {
        playerReference = GameManager.Instance.playerController.gameObject;

        playerH = playerReference.gameObject.GetComponent<Health>();
        playerSt = playerReference.gameObject.GetComponent<PlayerStamina>();
        playerOxygen = playerReference.gameObject.GetComponent<PlayerOxygen>();
    }

    private void Update()
    {
        healthBar.value = playerH.CurrentHealth;
        staminaBar.value = playerSt.CurrentStamina;
        oxygenBar.value = playerOxygen.CurrentOxygen;
    }
}

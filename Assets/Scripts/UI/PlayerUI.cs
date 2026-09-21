using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Stats UI Bar")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider staminaBar;
    [SerializeField] private Slider oxygenBar;

    private PlayerController playerController; 
    private GameObject playerReference;
    private Health playerH;
    private PlayerStamina playerSt;
    private PlayerOxygen playerOxygen;

    private void Start()
    {
        playerController = GameManager.Instance.playerController;

        if (playerController == null)
            return;

        playerReference = playerController.gameObject;
        playerH = playerReference.gameObject.GetComponent<Health>();
        playerSt = playerReference.gameObject.GetComponent<PlayerStamina>();
        playerOxygen = playerReference.gameObject.GetComponent<PlayerOxygen>();
    }

    private void Update()
    {
        healthBar.value = playerH.GetCurrentHealth();
        staminaBar.value = playerSt.GetCurrentStamina();
        oxygenBar.value = playerOxygen.GetCurrentOxygen();
    }
}

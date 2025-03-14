using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStatus : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private float maxHealth = 500f;
    private float currentHealth;

    [Header("Stamina")]
    [SerializeField] private StaminaBar staminaBar;
    [SerializeField] private float maxStamina = 50f;

    private void Start()
    {
        currentHealth = maxHealth;
        healthBar.ModifyHealth(currentHealth, maxHealth);
    }

    private void Update()
    {
        if(Gamepad.current.buttonNorth.wasPressedThisFrame)
            TakeDamage(5f);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthBar.ModifyHealth(currentHealth, maxHealth);
    }
}
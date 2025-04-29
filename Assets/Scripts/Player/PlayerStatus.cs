using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerStatus : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private HealthBar healthBar;
    private float maxHealth = 150f;
    private float currentHealth;

    [Header("Stamina")]
    [SerializeField] private StaminaBar staminaBar;
    private float maxStamina = 100f;
    public float currentStamina;

    private void Start()
    {
        currentHealth = maxHealth;
        healthBar.ModifyHealth(currentHealth, maxHealth);

        currentStamina = maxStamina;
        staminaBar.ModifyStamina(currentStamina, maxStamina);
    }

    private void Update()
    {
        
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthBar.ModifyHealth(currentHealth, maxHealth);
    }

    public void ConsumeStamina(float usedStamina)
    {
        currentStamina -= usedStamina;
        currentStamina = Math.Clamp(currentStamina, 0, maxStamina);
        staminaBar.ModifyStamina(currentStamina, maxStamina);
    }

    public void RefillStamina()
    {
        Debug.Log("Refills...");
        currentStamina += 2f;
        currentStamina = Math.Clamp(currentStamina, 0, maxStamina);
        staminaBar.ModifyStamina(currentStamina, maxStamina);
    }
}
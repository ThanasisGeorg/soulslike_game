using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class BearStatus : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private BearHealthBar bearHealthBar;
    private float maxHealth = 50f;
    public float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        bearHealthBar.ModifyHealth(currentHealth, maxHealth);
    }

    private void Update()
    {
        
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        bearHealthBar.ModifyHealth(currentHealth, maxHealth);
    }
}
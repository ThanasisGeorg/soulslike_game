using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemStatus : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private GolemHealthBar golemHealthBar;
    private float maxHealth = 50f;
    public float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        golemHealthBar.ModifyHealth(currentHealth, maxHealth);
    }

    private void Update()
    {
        
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        golemHealthBar.ModifyHealth(currentHealth, maxHealth);
    }
}

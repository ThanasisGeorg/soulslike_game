using UnityEngine;
using UnityEngine.UI;

public class BearHealthBar : MonoBehaviour
{
    [SerializeField] private Image currentHealthBar;

    public void ModifyHealth(float currentHealth, float maxHealth)
    {
        currentHealthBar.fillAmount = currentHealth / maxHealth;
    }
}

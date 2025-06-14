using UnityEngine;
using UnityEngine.UI;

public class GolemHealthBar : MonoBehaviour
{
    [SerializeField] private Image currentHealthBar;

    public void ModifyHealth(float currentHealth, float maxHealth)
    {
        currentHealthBar.fillAmount = currentHealth / maxHealth;
    }
}

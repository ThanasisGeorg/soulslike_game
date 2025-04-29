using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [SerializeField] private Image currentStaminaBar;

    public void ModifyStamina(float currentStamina, float maxStamina)
    {
        currentStaminaBar.fillAmount = currentStamina / maxStamina;
    }
}

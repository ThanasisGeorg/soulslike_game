using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowContinueBUtton : MonoBehaviour
{
    [SerializeField] private GameObject continueButton;

    private void Update()
    {
        Invoke(nameof(ShowContinueButton), 30f);
    }

    private void ShowContinueButton()
    {
        continueButton.SetActive(true);
    }
}

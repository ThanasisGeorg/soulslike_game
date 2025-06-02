using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Continue : MonoBehaviour
{
    void Start()
    {
        if (Keyboard.current.enabled == true)
        {
            Debug.Log("Keyboard");
            //Invoke(nameof(LoadScene), 0.3f);
        }
    }
    public void ContinueGame()
    {
        if (Gamepad.current != null)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                Debug.Log("A was pressed");
                Invoke(nameof(LoadScene), 0.3f);
            }
        }
        else if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                Debug.Log("A was pressed");
                Invoke(nameof(LoadScene), 0.3f);
            }
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(2);
    }
}

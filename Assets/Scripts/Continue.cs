using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Continue : MonoBehaviour
{   
    public void ContinueGame()
    {
        if(Gamepad.current.buttonSouth.wasPressedThisFrame)
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

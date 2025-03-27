using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitGame : MonoBehaviour
{
    public void QuitGame()
    {
        //SceneManager.UnloadSceneAsync(0);
        Application.Quit();
    }
}

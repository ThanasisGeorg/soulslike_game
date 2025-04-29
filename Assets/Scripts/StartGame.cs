using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void LoadGame()
    {
        Invoke(nameof(LoadScene), 0.3f);
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(1);
    }
}

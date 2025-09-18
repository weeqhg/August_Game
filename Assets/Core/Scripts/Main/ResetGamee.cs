using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetGame : MonoBehaviour
{
    public string nameScene;
    public void ResetGames()
    {
        SceneManager.LoadScene(nameScene);
    }
}

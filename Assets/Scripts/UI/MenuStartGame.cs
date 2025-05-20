using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuStartGame : MonoBehaviour
{
    [SerializeField] private string mainSceneName = "SampleScene"; // Set your scene name in Inspector

    public void StartGame()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game"); // Will only show in editor
        Application.Quit();     // Will work in a built version
    }
}
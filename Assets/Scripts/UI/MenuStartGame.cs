using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuStartGame : MonoBehaviour
{
    [SerializeField] private string mainSceneName = "SampleScene";

    public void StartGame()
    {
        MusicManager.Instance.FadeOutMusic();
        SceneManager.LoadScene(mainSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game"); 
        Application.Quit();    
    }
}
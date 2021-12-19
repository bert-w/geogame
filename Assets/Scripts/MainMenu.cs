using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public GameObject mainMenu;
    public GameObject howToMenu;

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void HowToGame()
    {
        mainMenu.SetActive(false);
        howToMenu.SetActive(true);
    }

    public void ReturnToMenu()
    {
        mainMenu.SetActive(true);
        howToMenu.SetActive(false);
    }

}

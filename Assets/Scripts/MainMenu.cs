using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{

    public GameObject mainMenu;
    public GameObject howToMenu;

    public Toggle geomExplanations;


    public void PlayGame()
    {
        PlayerScore.ResetScore();
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

    public void ChangeExplanations()
    {
        PlayerScore.explanations = geomExplanations.isOn;
     
    }


}

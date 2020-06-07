using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public Text levelText;
    public Text highScoreText;
    public Text highScoreText2;
    public Text highScoreText3;

    public Text lastScore;

    // Start is called before the first frame update
    void Start()
    {
        if (levelText != null)
        {
            levelText.text = "0";
        }
        //PlayerPrefs.SetInt("highscore", 0);
        if (highScoreText != null)
        {
            highScoreText.text = PlayerPrefs.GetInt("highscore").ToString();
        }
        if (highScoreText2 != null)
        { 
            highScoreText2.text = PlayerPrefs.GetInt("highscore2").ToString();
        }
        if (highScoreText3 != null)
        {
            highScoreText3.text = PlayerPrefs.GetInt("highscore3").ToString();
        }
        if (lastScore != null)
        {
            lastScore.text = PlayerPrefs.GetInt("lastscore").ToString();
        }
        
    }

    public void PlayGame()
    {
        if (Game.startingLevel == 0)
        {
            Game.startingAtLevelZero = true;
        } else
        {
            Game.startingAtLevelZero = false;
        }

        SceneManager.LoadScene("Level");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ChangeValue(float value)
    {
        Game.startingLevel = (int)value;
        levelText.text = value.ToString();
    }

    public void LaunchGameMenu()
    {
        SceneManager.LoadScene("GameMenu");
    }
}

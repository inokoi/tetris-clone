using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;

public class Game : MonoBehaviour
{

    public static int gridWidth = 10;
    public static int gridHeight = 20;

    public static Transform[,] grid = new Transform[gridWidth, gridHeight];

    public static bool startingAtLevelZero;
    public static int startingLevel;

    public Canvas hud_canvas;
    public Canvas pause_canvas;

    public int scoreOneLine = 40;
    public int scoreTwoLine = 100;
    public int scoreThreeLine = 300;
    public int scoreFourLine = 1200;

    public int currentLevel = 0;
    private int numLinesCleared = 0;

    public static float fallSpeed = 1.0f;
    public static bool isPaused = false;

    public AudioClip clearedLineSound;

    private int numberOfRowsThisTurn = 0;

    public Text hud_score;
    public Text hud_level;
    public Text hud_lines;

    private AudioSource audioSource;

    public static int currentScore = 0;

    private GameObject previewTetromino;
    private GameObject nextTetromino;
    private GameObject savedTetromino;

    private bool gameStarted = false;
    private int startingHighScore;
    private int startingHighScore2;
    private int startingHighScore3;

    private Vector2 previewTetrominoPosition = new Vector2(-6.5f, 16);
    private Vector2 savedTetrominoPosition = new Vector2(-6.5f, 10);

    public int maxSwaps = 2;
    private int currentSwaps = 0;

    // Start is called before the first frame update
    void Start()
    {
        currentScore = 0;
        hud_score.text = "0";

        currentLevel = startingLevel;

        hud_level.text = currentLevel.ToString();
        hud_lines.text = "0";

        SpawnNextTetromino();

        audioSource = GetComponent<AudioSource>();

        startingHighScore = PlayerPrefs.GetInt("highscore");
        startingHighScore2 = PlayerPrefs.GetInt("highscore2");
        startingHighScore3 = PlayerPrefs.GetInt("highscore3");

    }

    private void Update()
    {
        UpdateScore();
        UpdateUI();
        UpdateLevel();
        UpdateSpeed();
        CheckUserInput();
    }

    void CheckUserInput()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            if (Time.timeScale == 1)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
            //Time.timeScale = 0;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            GameObject tempNextTetromino = GameObject.FindGameObjectWithTag("currentActiveTetromino");

            SaveTetromino(tempNextTetromino.transform);
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0;
        audioSource.Pause();
        isPaused = true;

        hud_canvas.enabled = false;
        pause_canvas.enabled = true;

        Camera.main.GetComponent<Blur>().enabled = true;
    }

    void ResumeGame()
    {
        Time.timeScale = 1;
        audioSource.Play();
        isPaused = false;

        hud_canvas.enabled = true;
        pause_canvas.enabled = false;

        Camera.main.GetComponent<Blur>().enabled = false;
    }

    void UpdateLevel()
    {
        if (startingAtLevelZero == true || startingAtLevelZero == false && numLinesCleared / 10 > startingLevel)
        {
            currentLevel = numLinesCleared / 10;
        }

        //Debug.Log("Current Level :" + currentLevel);
    }

    void UpdateSpeed()
    {
        fallSpeed = 1.0f - ((float)currentLevel * 0.1f);
        //Debug.Log("Current fall speed :" + fallSpeed);
    }

    public void UpdateUI()
    {
        hud_score.text = currentScore.ToString();
        hud_level.text = currentLevel.ToString();
        hud_lines.text = numLinesCleared.ToString();
    }

    public void UpdateScore()
    {
        if (numberOfRowsThisTurn > 0)
        {
            if (numberOfRowsThisTurn == 1)
            {
                ClearedOneLine();
            }
            else if (numberOfRowsThisTurn == 2)
            {
                ClearedTwoLines();
            }
            else if (numberOfRowsThisTurn == 3)
            {
                ClearedThreeLines();
            }
            else if (numberOfRowsThisTurn == 4)
            {
                ClearedFourLines();
            }
            numberOfRowsThisTurn = 0;

            PlayLineClearedSound();
        }
    }

    public void ClearedOneLine()
    {
        currentScore += scoreOneLine + (currentLevel * 20);
        numLinesCleared++;
    }

    public void ClearedTwoLines()
    {
        currentScore += scoreTwoLine + (currentLevel * 25);
        numLinesCleared += 2;
    }
    public void ClearedThreeLines()
    {
        currentScore += scoreThreeLine + (currentLevel * 30);
        numLinesCleared += 3;
    }
    public void ClearedFourLines()
    {
        currentScore += scoreFourLine + (currentLevel * 40);
        numLinesCleared += 4;
    }

    public void UpdateHighScore()
    {
        if (currentScore > startingHighScore)
        {
            PlayerPrefs.SetInt("highscore3", startingHighScore2);
            PlayerPrefs.SetInt("highscore2", startingHighScore);
            PlayerPrefs.SetInt("highscore", currentScore);

        }
        else if (currentScore > startingHighScore2)
        {
            PlayerPrefs.SetInt("highscore3", startingHighScore2);
            PlayerPrefs.SetInt("highscore2", currentScore);
        }
        else if (currentScore > startingHighScore3)
        {
            PlayerPrefs.SetInt("highscore3", currentScore);
        }

        PlayerPrefs.SetInt("lastscore", currentScore);
    }

    public void PlayLineClearedSound()
    {
        audioSource.PlayOneShot(clearedLineSound);
    }

    public bool CheckIsValidPosition(GameObject tetromino)
    {
        foreach (Transform mino in tetromino.transform)
        {
            Vector2 pos = round(mino.position);
            if (!CheckIsInsideGrid(pos))
            {
                return false;
            }
            if (GetTransformAtGridPosition(pos) != null && GetTransformAtGridPosition(pos).parent != tetromino.transform)
            {
                return false;
            }
        }

        return true;
    }

    public bool CheckIsAboveGrid(Tetromino tetromino)
    {
        for (int x = 0; x < gridWidth; ++x)
        {
            foreach (Transform mino in tetromino.transform)
            {
                Vector2 pos = round(mino.position);

                if (pos.y > gridHeight - 1)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsFullRowAt(int y)
    {
        // parametr y, is the row we will iterate over in the grid array to check each x position for a transform
        for (int x = 0; x < gridWidth; ++x)
        {
            // if we found a position that return NULL instead of transform, then we know that the row is not full
            if (grid[x, y] == null)
            {
                return false;
            }
        }
        // since we found a full row, we increment the full row variable.
        numberOfRowsThisTurn++;

        return true;
    }

    public void DeleteMinoAt(int y)
    {
        for (int x = 0; x < gridWidth; ++x)
        {
            Destroy(grid[x, y].gameObject);

            grid[x, y] = null;
        }
    }

    public void MoveRowDown(int y)
    {
        for (int x = 0; x < gridWidth; ++x)
        {
            if(grid[x, y] != null)
            {
                grid[x, y -1] = grid[x, y];

                grid[x, y] = null;

                grid[x, y -1].position += new Vector3(0, -1, 0);
            }
        }
    }

    public void MoveAllRowsDown(int y)
    {
        for (int i = y; i < gridHeight; ++i)
        {
            MoveRowDown(i);
        }
    }

    public void DeleteRow()
    {
        for (int y = 0; y < gridHeight; ++y)
        {
            if(IsFullRowAt(y))
            {
                DeleteMinoAt(y);

                MoveAllRowsDown(y + 1);

                --y;
            }
        }
    }

    public void UpdateGrid(Tetromino tetromino)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; ++x)
            {
                if (grid[x, y] != null) 
                {
                    if (grid[x, y].parent == tetromino.transform)
                    {
                        grid[x, y] = null;
                    }
                }
            }
        }

        foreach (Transform mino in tetromino.transform)
        {
            Vector2 pos = round(mino.position);
            if (pos.y < gridHeight)
            {
                grid[(int)pos.x, (int)pos.y] = mino;
            }
        }
    }

    public Transform GetTransformAtGridPosition(Vector2 pos)
    {
        if (pos.y > gridHeight -1)
        {
            return null;
        } else
        {
            return grid[(int)pos.x, (int)pos.y];
        }
    }

    public void SpawnNextTetromino()
    {
        if(!gameStarted)
        {
            gameStarted = true;

            nextTetromino = (GameObject)Instantiate(Resources.Load(GetRandomTetromino(), typeof(GameObject)), new Vector2(5.0f, 20.0f), Quaternion.identity);
            previewTetromino = (GameObject)Instantiate(Resources.Load(GetRandomTetromino(), typeof(GameObject)), previewTetrominoPosition, Quaternion.identity);
            previewTetromino.GetComponent<Tetromino>().enabled = false;
            nextTetromino.tag = "currentActiveTetromino";
        } else {
            previewTetromino.transform.localPosition = new Vector2(5.0f, 20.0f);
            nextTetromino = previewTetromino;
            nextTetromino.GetComponent<Tetromino>().enabled = true;
            nextTetromino.tag = "currentActiveTetromino";
            previewTetromino = (GameObject)Instantiate(Resources.Load(GetRandomTetromino(), typeof(GameObject)), previewTetrominoPosition, Quaternion.identity);
            previewTetromino.GetComponent<Tetromino>().enabled = false;
        }

        currentSwaps = 0;
    }

    public void SaveTetromino(Transform t)
    {
        currentSwaps++;
        if (currentSwaps > maxSwaps)
        {
            return;
        }

        if (savedTetromino != null)
        {
            // There is currently a tetromino being held

            GameObject tempSavedTetromino = GameObject.FindGameObjectWithTag("currentSavedTetromino");

            tempSavedTetromino.transform.localPosition = new Vector2(gridWidth / 2, gridHeight);

            if(!CheckIsValidPosition(tempSavedTetromino))
            {
                tempSavedTetromino.transform.localPosition = savedTetrominoPosition;

                return;
            }

            savedTetromino = (GameObject)Instantiate(t.gameObject);
            savedTetromino.GetComponent<Tetromino>().enabled = false;
            savedTetromino.transform.localPosition = savedTetrominoPosition;//new Vector2(gridWidth / 2, gridHeight);//savedTetrominoPosition;
            savedTetromino.tag = "currentSavedTetromino";

            nextTetromino = (GameObject)Instantiate(tempSavedTetromino);
            nextTetromino.GetComponent<Tetromino>().enabled = true;
            nextTetromino.transform.localPosition = new Vector2(gridWidth / 2, gridHeight);
            nextTetromino.tag = "currentActiveTetromino";

            DestroyImmediate(t.gameObject);
            DestroyImmediate(tempSavedTetromino);

        }
        else
        {
            // There is currently no tetromino being held

            savedTetromino = (GameObject)Instantiate(GameObject.FindGameObjectWithTag("currentActiveTetromino"));
            savedTetromino.GetComponent<Tetromino>().enabled = true;
            savedTetromino.transform.localPosition = savedTetrominoPosition;
            savedTetromino.tag = "currentSavedTetromino";

            DestroyImmediate(GameObject.FindGameObjectWithTag("currentActiveTetromino"));

            SpawnNextTetromino();
        }
    }

    public bool CheckIsInsideGrid(Vector2 pos)
    {
        return ((int)pos.x >= 0 && (int)pos.x < gridWidth && (int)pos.y >= 0);
    }

    public Vector2 round(Vector2 pos)
    {
        return new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
    }

    string GetRandomTetromino()
    {
        int randomTetromino = Random.Range(1, 9);

        string randomTetrominoName = "Prefabs/Tetromino_1";

        switch (randomTetromino)
        {
            case 1:
                randomTetrominoName = "Prefabs/Tetromino_1";
                break;
            case 2:
                randomTetrominoName = "Prefabs/Tetromino_2";
                break;
            case 3:
                randomTetrominoName = "Prefabs/Tetromino_3";
                break;
            case 4:
                randomTetrominoName = "Prefabs/Tetromino_4";
                break;
            case 5:
                randomTetrominoName = "Prefabs/Tetromino_5";
                break;
            case 6:
                randomTetrominoName = "Prefabs/Tetromino_6";
                break;
            case 7:
                randomTetrominoName = "Prefabs/Tetromino_7";
                break;
            case 8:
                randomTetrominoName = "Prefabs/Tetromino_8";
                break;
        }

        return randomTetrominoName;
    }

    public void GameOver()
    {
        //hud_score.text = "0";
        //hud_level.text = "0";
        //hud_lines.text = "0";
        UpdateHighScore();
        SceneManager.LoadScene("GameOver");
    }
}

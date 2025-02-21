using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Static instance for the singleton
    public static GameManager Instance { get; private set; }

    public int GameID = 0;

    public GameObject GameOverScreen, GameWinScreen, InfoScreen;
    public bool GameState = false;
    public BasePlayer Player;

    private ScoreObj Score;

    public GameObject Wallet;
    public float[] xPositions = { -1.8f, 0f, 1.8f };
    public float movementDuration = 1f;
    private Vector3 initialWalletPosition;
    public Vector3 targetPosition;
    public bool isMoved = false;
    public AudioSource[] coinAudio;

    public Text ScoreText;
    private int currentScore;
    public ParticleSystem poof;
    public GameObject[] item;

    [DllImport("__Internal")]
    private static extern void SendScore(int score, int game);

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Another instance of GameManager already exists. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    private void Start()
    {
        initialWalletPosition = Wallet.transform.position;
        InfoScreen.SetActive(true);
        ScoreText.text = "0";
        StartCoroutine(MoveCupidRandomly());
    }

    void Update()
    {
        if (!GameState)
            return;

        //GAME LOGIC
        UpdateItemsBasedOnScore();
    }

    private IEnumerator MoveCupidRandomly()
    {
        float lastXPosition = Wallet.transform.position.x; // Store the last position

        while (true)
        {
            yield return new WaitForSeconds(5f); // Wait before moving Wallet

            if (Wallet != null)
            {
                float targetX;

                // Ensure the new position is different from the last one
                do
                {
                    targetX = xPositions[Random.Range(0, xPositions.Length)];
                }
                while (Mathf.Approximately(targetX, lastXPosition)); // Avoid selecting the same position

                targetPosition = new Vector3(targetX, initialWalletPosition.y, initialWalletPosition.z);
                lastXPosition = targetX; // Update last position

                float elapsedTime = 0f;
                Vector3 startPosition = Wallet.transform.position;
               
                while (elapsedTime < movementDuration)
                {
                    isMoved = true;
                    Wallet.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / movementDuration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                Wallet.transform.position = targetPosition; // Ensure final position is set correctly
            }
        }
    }

    public void GameWin()
    {
        GameState = false;
        GameWinScreen.SetActive(true);
        SendScore(currentScore, 80);
    }

    public void GameOVer()
    {
        GameState = false;
        GameOverScreen.SetActive(true);
      //  Debug.Log(currentScore);
        SendScore(currentScore, 80);
    }

    public void GameResetScreen()
    {
        ScoreText.text = "0";
        currentScore = 0;
        Score.score = 0;
        InfoScreen.SetActive(false);
        GameOverScreen.SetActive(false);
        GameWinScreen.SetActive(false);
        GameState = true;
        Player.Reset();
        Player.isToss = false;

        foreach (var itemObject in item)
        {
            itemObject.SetActive(false);
        }
    }

    /* public void AddScore(float f)
     {
         Score.score += f;
     }*/
    private void UpdateItemsBasedOnScore()
    {
        // Loop through the items and check if they should be activated based on the score
        for (int i = 0; i < item.Length; i++)
        {
            // Check if the current score has reached the threshold for this item
            if (currentScore >= 10 + (i * 30) && !item[i].activeSelf)
            {
                // Activate the item
                item[i].SetActive(true);
            }
        }
    }

    public void AddScore()
    {


        if (int.TryParse(ScoreText.text, out currentScore))
        {
            currentScore += 10;
            ScoreText.text = currentScore.ToString();
        }
        else
        {

            ScoreText.text = "0";
        }
    }

    public void DeleteScore()
    {


        if (int.TryParse(ScoreText.text, out currentScore))
        {
            currentScore -= 5;
            ScoreText.text = currentScore.ToString();
        }
       
    }


    //HELPER FUNTION TO GET SPAWN POINT
    public Vector2 GetRandomPointInsideSprite(SpriteRenderer SpawnBounds)
    {
        if (SpawnBounds == null || SpawnBounds.sprite == null)
        {
            Debug.LogWarning("Invalid sprite renderer or sprite.");
            return Vector2.zero;
        }

        Bounds bounds = SpawnBounds.sprite.bounds;
        Vector2 randomPoint = new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y)
        );

        // Transform local point to world space
        return SpawnBounds.transform.TransformPoint(randomPoint);
    }


    public struct ScoreObj
    {
        public float score;
    }
}

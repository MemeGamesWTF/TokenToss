using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class BasePlayer : MonoBehaviour
{

    private Player_Controls playercontrol;
    private Vector2 startTouchPosition;
    public Rigidbody2D fishRigidbodies;
    public BoxCollider2D fishColliders;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float tossForce = 10f;
    public bool isToss = false;
    public GameObject Enemy;
    private Vector3 initialPosition;
    public GameObject[] CoinList;
    public LineRenderer lineRenderer;
    // Enemy Movement Variables
    // public GameObject Enemy;
    // private Vector3 initialPosition;
    public float enemySpeed = 2f;
    private float leftLimit = -2.4f;
    private float rightLimit = 2.4f;
    private int enemyDirection = 1; // 1 moves right, -1 moves left
    private bool isTouch = false;
    public GameManager manager;

    

    private void Awake()
    {
        playercontrol = new Player_Controls();
        // fishRigidbodies = new Rigidbody2D();
    }

    private void OnEnable()
    {
        playercontrol.Enable();
        playercontrol.Player.Toss.performed += OnTouch;
    }

    private void OnDisable()
    {
        playercontrol.Player.Toss.performed -= OnTouch;
        playercontrol.Disable();
    }

    void Start()
    {
        
        initialPosition = transform.position;
        lineRenderer.enabled = false;

    }

    void Update()
    {
        if (!GameManager.Instance.GameState)
            return;

        if ((Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) ||
             (Mouse.current != null && Mouse.current.leftButton.isPressed))
        {
        }
        else
        {
            HandleToss();
        }
        //UPDATE LOGIC

        MoveEnemy();
    }

    private void HandleToss()
    {
        if (isToss)
        {
            isTouch = true;
            Rigidbody2D selectedFishRb = fishRigidbodies;
            BoxCollider2D selectedFishCollider = fishColliders;
            //  LineRenderer lineRenderer = fishLineRenderers;
            if (selectedFishRb != null)
            {


                /*  Animator selectedFishAnimator = spawnedFishes.GetComponentInChildren<Animator>();
                  if (selectedFishAnimator != null)
                  {

                      selectedFishAnimator.SetBool("isJump", true);
                  }*/

                float rotationInRadians = selectedFishRb.rotation * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(-Mathf.Sin(rotationInRadians), Mathf.Cos(rotationInRadians));

                float xAdjustment = Mathf.Clamp01(Mathf.Abs(direction.x));
                direction.x = xAdjustment * Mathf.Sign(direction.x);

                selectedFishRb.linearVelocity = direction * tossForce;

               // selectedFishCollider.size = new Vector2(1.3f, 1.3f);
                lineRenderer.enabled = false;

                if (selectedFishRb.position.y < -3)
                {
                      manager.coinAudio[1].Play();
                }
            }
        }

    }

    public void OnTouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 touchPosition;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                touchPosition = Mouse.current.position.ReadValue();
            }
            else
            {
                return; // No valid input
            }

            if (startTouchPosition == Vector2.zero)
            {
                startTouchPosition = touchPosition;
            }
            else
            {
                float deltaX = touchPosition.x - startTouchPosition.x;
                //  Debug.Log(deltaX);
                if (deltaX > 0)
                {
                    moveRotation(2f);
                }
                else if (deltaX < 0)
                {
                    moveRotation(-2f);
                }

                startTouchPosition = touchPosition;
            }
        }
    }

    private void moveRotation(float rotationAmount)
    {
        if (!isTouch)
        {
           

            Rigidbody2D selectedFishRb = fishRigidbodies;
            BoxCollider2D selectedFishCollider = fishColliders;

            if (selectedFishRb != null && selectedFishCollider != null)
            {
                float targetRotation = selectedFishRb.rotation + rotationAmount;
                targetRotation = Mathf.Clamp(targetRotation, -20f, 20f);

                float smoothRotation = Mathf.Lerp(selectedFishRb.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                selectedFishRb.rotation = smoothRotation;
                lineRenderer.enabled = true;
            }

            isToss = true;

        }
    }
    




private void MoveEnemy()
    {
        if (Enemy != null)
        {
            // Move the enemy left and right
            Enemy.transform.position += Vector3.right * enemyDirection * enemySpeed * Time.deltaTime;

            // Change direction at boundaries
            if (Enemy.transform.position.x >= rightLimit)
            {
                enemyDirection = -1; // Move left
            }
            else if (Enemy.transform.position.x <= leftLimit)
            {
                enemyDirection = 1; // Move right
            }
        }
    }



    public void GameOver()
    {

        GameManager.Instance.GameOVer();
    }

    public void Reset()
    {
       
        ResetPlayerPosition();
        ActivateRandomCoin();


      //  StopCoroutine(MoveCupidRandomly());
      //  StartCoroutine(MoveCupidRandomly());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Barrier"))
        {
            gameObject.SetActive(false);
            //  Debug.Log("Gamve over");
            GameManager.Instance.DeleteScore();
            ResetPlayerPosition();

        }
        if (collision.gameObject.CompareTag("Wallet"))
        {
            GameManager.Instance.AddScore();
            manager.poof.Play();
            manager.coinAudio[0].Play();
            gameObject.SetActive(false);
            ResetPlayerPosition();
            ActivateRandomCoin();
        }

       
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            gameObject.SetActive(false);
            //Debug.Log("Gamve over");
            manager.coinAudio[2].Play();
            GameOver();
            ActivateRandomCoin();
            ResetPlayerPosition();
        }
    }

    public void ResetPlayerPosition()
    {
        isTouch = false;
        transform.position = initialPosition; // Move back to the initial position
        fishRigidbodies.linearVelocity = Vector2.zero; // Reset velocity
        fishRigidbodies.angularVelocity = 0f; // Reset rotation
        fishRigidbodies.rotation = 0f;
        gameObject.SetActive(true);
        isToss = false;
       
    }

    private void ActivateRandomCoin()
    {
        if (CoinList.Length == 0) return;

        // Deactivate all coins first
        foreach (GameObject coin in CoinList)
        {
            coin.SetActive(false);
        }

        // Choose a random coin and activate it
        int randomIndex = Random.Range(0, CoinList.Length);
        CoinList[randomIndex].SetActive(true);
    }
}

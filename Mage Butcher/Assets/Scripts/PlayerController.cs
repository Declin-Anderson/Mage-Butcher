using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the player info and player inputs
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Player Attributes")]
    // Movespeed of the character
    public float moveSpeed = 5f;
    // Sensitivity of mouse and left stick
    public float lookSensitvity = 100f;
    // Height the player will jump
    public float jumpHeight = 5f;

    [Header("References")]
    // reference to the character controller on the player object
    public CharacterController playerController;
    // reference to the camera position 
    public Transform cameraPosition;

    // The input of movement that the player has
    private Vector2 movementInput;
    // The Input of looking around that the player has
    private Vector2 lookInput;
    // If the player did jump
    private bool didJump;
    // The velocity that the player has
    private Vector3 velocity;
    // The gravity affecting the player
    private float gravity = -9.8f;
    // The yRotation of the player to prevent wrongful rotation
    private float yRotation = 0f;

    // Reference to the Input action map script
    private PlayerControls playerInputs;

    private void Awake()
    {
        playerInputs = new PlayerControls();

        // Reads the inputs for moves when they are performed by the player and when they are cancelled using context to get feedback from the input
        playerInputs.Player.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        playerInputs.Player.Move.canceled += ctx => movementInput = Vector2.zero;

        // Reads the inputs for looking when they are performed by the player and when they are cancelled using context to get feedback from the input
        playerInputs.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        playerInputs.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        // Reads the jump input for the player
        playerInputs.Player.Jump.performed += ctx => OnJump();
    }

    /// <summary>
    /// When the object is enable it turns on the player inputs
    /// </summary>
    private void OnEnable() => playerInputs.Enable();
    
    /// <summary>
    /// When the object is disabled it turns off the player inputs
    /// </summary>
    private void OnDisable() => playerInputs.Disable();

    /// <summary>
    /// When the object is initial started
    /// </summary>
    private void Start()
    {
        // Lock the cursor to the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleLook();
        HandleGravity();
    }

    /// <summary>
    /// Handles the movement input of the player by updating their transform based on the input of the player
    /// </summary>
    private void HandleMovement()
    {
        // Getting the direction input of the player for which way they will be moving
        Vector3 move = transform.right * movementInput.x + transform.forward * movementInput.y;
        // Calling the Move function on the palyer controls script to move the player based on the movement vector
        playerController.Move(move * moveSpeed * Time.deltaTime);

        // Adding a velocity to the player upwards if they did the jump input
        playerController.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Handles the view of the player based on how they moved the mouse or right stick on a controller
    /// </summary>
    private void HandleLook()
    {
        // Getting the x and y of the current movement and then updating it with the sensitivity and deltatime
        float mouseX = lookInput.x * lookSensitvity * Time.deltaTime;
        float mouseY = lookInput.y * lookSensitvity * Time.deltaTime;

        // Changing the rotation of the y based on the y input of the player
        yRotation -= mouseY;
        yRotation = Mathf.Clamp(yRotation, -90f, 90f);

        // changing the rotation of the camera and player based on the change in vision
        cameraPosition.localRotation = Quaternion.Euler(yRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    /// <summary>
    /// Handles the gravity on the player based on if they have jumped or not
    /// </summary>
    private void HandleGravity()
    {
        // If the player is not on the ground apply gravity to the player
        if(playerController.isGrounded)
        {
            if(didJump)
            {
                velocity.y = jumpHeight;
                didJump = false;
            }
            else if(velocity.y < 0)
            {
                velocity.y = -2f;
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
    }

    /// <summary>
    /// Handles the logic of jumping when the jump button is pressed
    /// </summary>
    private void OnJump()
    {
        if(playerController.isGrounded)
        {
            didJump = true;
        }
    }
}

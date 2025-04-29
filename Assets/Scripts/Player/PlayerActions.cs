using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.AI;
using Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerActions : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private Transform playerObj;
    [SerializeField] private GameObject playerCam;
    [SerializeField] private Transform orientation;
    [SerializeField] private BoxCollider shieldCollider;
    [SerializeField] private CapsuleCollider swordCollider;

    [Header("Bear")]
    [SerializeField] private Transform bearObj;

    [Header("Movement")]
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask ground;

    [Header("Menu")]
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject inventory;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject youDiedPanel;

    [Header("Quit")]
    [SerializeField] private GameObject quitPanel;
    [SerializeField] private GameObject noButton;
    [SerializeField] private GameObject yesButton;

    [Header("Player Input")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Event System")]
    [SerializeField] private EventSystem eventSystem;

    [Header("Cinemachine Camera")]
    [SerializeField] private GameObject cinemachineCamera;

    [Header("Layers")]
    [SerializeField] private LayerMask enemy;

    private float tapDuration = 0.2f;
    private bool rbPressedAgain = false;
    private float buttonEastPressTime = 0f;
    private bool buttonEastHeld = false;
    private bool grounded;
    private bool readyToBackStep = true;
    private bool readyToRoll = true;
    private bool readyToAttack1 = true;
    private bool readyToBlock = true;
    private bool readyToBlockWhileWalking = true;
    private bool readyToFall = false;
    private bool readyToSprint = true;
    private bool isAttacking = false;
    private bool menuIsOpen = false;
    private bool inventoryIsOpen = false;
    private bool settingsAreOpen = false;
    private bool isLocking = false;
    private bool timeToDie = false;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;
    private CinemachineFreeLook cfl;
    private Transform ct;

    private PlayerStatus playerStatus; 
    private Animator _animator;

    private NavMeshAgent m_Agent;
    private RaycastHit m_HitInfo = new();

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        cfl = cinemachineCamera.GetComponent<CinemachineFreeLook>();
        ct = cinemachineCamera.GetComponent<Transform>();
        
        m_Agent = GetComponent<NavMeshAgent>();

        playerStatus = FindObjectOfType<PlayerStatus>();
        _animator = playerObj.GetComponentInChildren<Animator>();
    }

    private void ReloadGame()
    {
        SceneManager.UnloadSceneAsync(2);
        SceneManager.LoadScene(2);
    }

    private void ShowYouDied()
    {
        youDiedPanel.SetActive(true);
    }

    private void Update()
    {
        if(playerStatus.currentHealth <= 0f)
        {
            _animator.SetBool("timeToDie", true);
            timeToDie = true;
            playerInput.DeactivateInput();
            Invoke(nameof(ShowYouDied), 1f);
            Invoke(nameof(ReloadGame), 4f);
        }
        else
        {
            PlayerInput();
            
            Debug.Log("Grounded: " + IsGrounded());
            // ground check
            if(IsGrounded())
            {
                rb.drag = groundDrag;        
            }
            else 
            {
                rb.drag = 0;
            }
        }
    }

    private void FixedUpdate()
    {
        if(timeToDie == false)
        {
            if(!isAttacking)
            {
                if(!isLocking)
                {
                    Rotate();
                } 
                else if(isLocking)
                {
                    RotateWhenLocking();
                }
                Run(); 
            }
        }
    }

    private bool IsGrounded()
    {
        return grounded = Physics.Raycast(playerObj.position, Vector3.down, playerHeight / 2, ground);
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        StartButtonHandling();
        RightStickButtonHandling();

        if(Gamepad.current.leftStick.ReadValue().magnitude > 0.1f && grounded)
        {
            ButtonEastHandlingWhileMoving();
            if(Gamepad.current.leftShoulder.isPressed)
            {
                BlockWhileWalking();
            }   
        }
        else if(Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            if(inventoryIsOpen)
            {
                inventoryPanel.SetActive(false);
                inventoryIsOpen = !inventoryIsOpen;
            }
            else if(settingsAreOpen)
            {
                settingsPanel.SetActive(false);
                settingsAreOpen = !settingsAreOpen;
            }
            else 
            {
                Backstep();
            }
        }
        if(Gamepad.current.rightShoulder.wasPressedThisFrame)
        {
            Debug.Log("Time to attack 1");
            Attack1();
            /*if(!isAttacking)
            {
                Attack1();
            }
            else if(isAttacking && readyToAttack1 == false)
            {
                Debug.Log("RB pressed again");
                rbPressedAgain = true;
                DoubleAttackHandling();
            }*/
        }
        if(Gamepad.current.leftShoulder.isPressed && grounded)
        {
            Debug.Log("Time to block");
            Block();
        }
        if(Gamepad.current.leftStick.ReadValue().magnitude == 0 || Gamepad.current.buttonEast.wasReleasedThisFrame)
        {
            ResetSprint();
        }
        if(Gamepad.current.leftShoulder.wasReleasedThisFrame)
        {
            ResetBlock();
        }
        if(Gamepad.current.leftStick.ReadValue().magnitude == 0 || Gamepad.current.leftShoulder.wasReleasedThisFrame)
        {
            ResetBlockWhileWalking();
        }
        if(Gamepad.current.buttonNorth.wasPressedThisFrame)
        {
            playerStatus.TakeDamage(10f);
        }
        if(playerStatus.currentStamina < 100f && Gamepad.current.buttonEast.ReadValue() == 0) 
            if(readyToRoll == false || readyToBackStep == false || readyToAttack1 == false)
                Invoke(nameof(playerStatus.RefillStamina), 1f);
            else playerStatus.RefillStamina();
    }

    public void InventoryClicked()
    {
        menu.SetActive(false);
        menuIsOpen = !menuIsOpen;
        inventoryPanel.SetActive(true);
        inventoryIsOpen = !inventoryIsOpen;
    }

    public void SettingsClicked()
    {
        menu.SetActive(false);
        menuIsOpen = !menuIsOpen;
        settingsPanel.SetActive(true);
        settingsAreOpen = !settingsAreOpen;
    }

    public void QuitClicked()
    {
        menu.SetActive(false);
        menuIsOpen = !menuIsOpen;
        quitPanel.SetActive(true);
        eventSystem.SetSelectedGameObject(noButton);
    }

    public void YesClicked()
    {
        SceneManager.UnloadSceneAsync(2);
        SceneManager.LoadScene(0);
    }

    public void NoClicked()
    {
        quitPanel.SetActive(false);
    }

    private void StartButtonHandling()
    {
        if(Gamepad.current.startButton.wasPressedThisFrame)
        {  
            if(!menuIsOpen)
            {
                menu.SetActive(true);
                eventSystem.SetSelectedGameObject(inventory);
            }
            else if(menuIsOpen)
            {
                menu.SetActive(false);
            }

            menuIsOpen = !menuIsOpen;
        }
    }

    private void RightStickButtonHandling()
    {
        if(Gamepad.current.rightStickButton.wasPressedThisFrame)
        {   
            isLocking = !isLocking;
        }
    }

    private void ButtonEastHandlingWhileMoving()
    {
        if(Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            buttonEastPressTime = Time.time;
            buttonEastHeld = true;
        }

        if(Gamepad.current.buttonEast.wasReleasedThisFrame)
        {
            if((Time.time - buttonEastPressTime) < tapDuration)
            {
                Debug.Log("Time to roll");
                Roll();
                playerStatus.ConsumeStamina(20f);
            }

            buttonEastHeld = false;
        }

        if(buttonEastHeld && (Time.time - buttonEastPressTime) >= tapDuration)
        {
            if(playerStatus.currentStamina > 0)
            {
                Debug.Log("Time to sprint");
                Sprint();
                playerStatus.ConsumeStamina(0.5f);
            }
            else ResetSprint();
        }
    }

    private void Run()
    {
        // calculate movement direction
        moveDirection = horizontalInput * orientation.right + verticalInput * orientation.forward;

        // on ground
        if(grounded) 
        {
            rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);
        }
        // in air
        else if(!grounded)
            rb.AddForce(moveDirection * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        AnimateRun();
    }

    private void AnimateRun()
    {
        float distanceCovered = new Vector2(horizontalInput, verticalInput).magnitude;
        float speed = Mathf.Lerp(_animator.GetFloat("speed"), distanceCovered, Time.deltaTime * 10f);
        _animator.SetFloat("speed", speed);
    }

    private void Sprint()
    {
        if(!readyToSprint) return;

        readyToSprint = false;

        moveSpeed = sprintSpeed;
        //moveDirection = horizontalInput * orientation.right + verticalInput * orientation.forward;
        //rb.AddForce(moveDirection * sprintSpeed, ForceMode.Force);

        AnimateSprint();
    }

    private void AnimateSprint()
    {
        _animator.SetBool("readyToSprint", false);
    }

    private void ResetSprint()
    {
        moveSpeed = 5;
        readyToSprint = true;
        _animator.SetBool("readyToSprint", true);
    }

    private void Rotate()
    {
        // rotate orientation
        Vector3 viewDir = player.transform.position - new Vector3(playerCam.transform.position.x, player.transform.position.y, playerCam.transform.position.z);
        orientation.forward = viewDir; 

        // rotate player object
        Vector3 inputDir = horizontalInput * orientation.right + verticalInput * orientation.forward;

        // execute rotation
        if (inputDir != Vector3.zero) 
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
    }

    private void RotateWhenLocking()
    {
        Vector3 bearPosition = new(
            bearObj.position.x, 
            playerObj.position.y,
            bearObj.position.z
        );
        orientation.forward = -(playerObj.position - bearPosition);

        playerObj.LookAt(bearPosition);

        ct.forward = -(playerObj.position - bearPosition);
        ct.LookAt(bearPosition);
    }

    private void DisableColliders()
    {
        shieldCollider.enabled = false;
        swordCollider.enabled = false;
    }

    private void ResetColliders()
    {
        shieldCollider.enabled = true;
        swordCollider.enabled = true;
    }

    private void Roll()
    {
        if(!readyToRoll) return;

        readyToRoll = false;

        moveDirection = horizontalInput * orientation.right + verticalInput * orientation.forward + Vector3.up * 3f;

        AnimateRoll();

        Invoke(nameof(DisableColliders), 0.1f);

        rb.AddForce(moveDirection * (moveSpeed + 2f) * 10f, ForceMode.Force);

        Invoke(nameof(ResetColliders), 0.75f);        

        Invoke(nameof(ResetRoll), 1f);
    }

    private void AnimateRoll()
    {
        _animator.SetBool("readyToRoll", false);  
    }

    private void ResetRoll()
    {
        readyToRoll = true;
        _animator.SetBool("readyToRoll", true);  
    }

    private void Backstep()
    {
        if(!readyToBackStep) return;
        
        readyToBackStep = false;
        
        playerStatus.ConsumeStamina(20f);

        AnimateBackstep();
        Invoke(nameof(ExecuteBackstep), 0.15f);

        Invoke(nameof(ResetBackStep), 0.3f);
    }

    private void AnimateBackstep()
    {
        _animator.SetBool("readyToBackstep", false);  
    }   

    private void ExecuteBackstep()
    {
        // players orientation
        Vector3 inputDir = horizontalInput * orientation.right + verticalInput * orientation.forward;
        if (inputDir != Vector3.zero) 
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);

        // get back direction from player's orientation and add force
        Vector3 backDirection = -playerObj.forward;
        rb.velocity = Vector3.zero;
        rb.AddForce(backDirection * 40f, ForceMode.Impulse);
    }

    private void ResetBackStep()
    {
        readyToBackStep = true;
        _animator.SetBool("readyToBackstep", true);
    } 

    private void DoubleAttackHandling()
    {
        if(rbPressedAgain)
        {
            Attack2();
        }
        else
        {
            Invoke(nameof(ResetAttack1), 2.2f);
        }
    }

    private void Attack1()
    {
        readyToAttack1 = false;
        isAttacking = true;

        playerStatus.ConsumeStamina(40f);

        AnimateAttack1();
        Invoke(nameof(ExecuteAttack1), 0.5f);

        Invoke(nameof(ResetAttack1), 2.2f);
        //DoubleAttackHandling();
    }

    private void AnimateAttack1()
    {        
        _animator.SetBool("readyToAttack1", false);
    }

    private void ExecuteAttack1()
    {
        // players orientation
        Vector3 inputDir = horizontalInput * orientation.right + verticalInput * orientation.forward;
        if (inputDir != Vector3.zero) 
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);

        // get front direction from player's orientation and add force
        Vector3 frontDirection = playerObj.forward;

        rb.AddForce(frontDirection * 10f, ForceMode.Impulse);
    }

    private void ResetAttack1()
    {
        isAttacking = false;
        readyToAttack1 = true;
        rbPressedAgain = false;
        _animator.SetBool("readyToAttack1", true);
    }

    private void Attack2()
    {
        AnimateAttack2();
        Invoke(nameof(ExecuteAttack2), 0.5f);

        Invoke(nameof(ResetAttack2), 2.2f);
    }

    private void AnimateAttack2()
    {        
        _animator.SetBool("readyToAttack2", true);
    }

    private void ExecuteAttack2()
    {
        // players orientation
        Vector3 inputDir = horizontalInput * orientation.right + verticalInput * orientation.forward;
        if (inputDir != Vector3.zero) 
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);

        // get front direction from player's orientation and add force
        Vector3 frontDirection = playerObj.forward;

        rb.AddForce(frontDirection * 10f, ForceMode.Impulse);
    }

    private void ResetAttack2()
    {
        isAttacking = false;
        readyToAttack1 = true;
        rbPressedAgain = false;
        _animator.SetBool("readyToAttack2", false);
        _animator.SetBool("readyToAttack1", true);
    }

    private void Block()
    {
        if(!readyToBlock) return;

        AnimateBlock();
    }

    private void AnimateBlock()
    {
        _animator.SetBool("readyToBlock", false);
    }

    private void ResetBlock()
    {
        readyToBlock = true;
        _animator.SetBool("readyToBlock", true);
    }

    private void BlockWhileWalking()
    {
        if(!readyToBlockWhileWalking) return;

        _animator.SetBool("readyToBlockWhileWalking", true);
    }

    private void ResetBlockWhileWalking()
    {
        readyToBlockWhileWalking = true;
        _animator.SetBool("readyToBlockWhileWalking", false);
    }

    private void AnimateFalling()
    {
        readyToFall = true;
        _animator.SetBool("readyToFall", true);
    }

    private void ResetFalling()
    {
        readyToFall = false;
        _animator.SetBool("readyToFall", false);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
}
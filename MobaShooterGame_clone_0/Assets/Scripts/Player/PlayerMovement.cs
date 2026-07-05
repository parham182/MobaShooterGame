using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [Header("System")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float sprintSpeed = 10f;
    [SerializeField] float crouchSpeed = 3f;
    [SerializeField] float crouchSitSpeed = 3f;
    [SerializeField] float gravity = -20f;
    [SerializeField] float standHeight = 2f;
    [SerializeField] float crouchHeight = 1f;
    [SerializeField] float standCameraY = 0.8f;
    [SerializeField] float crouchCameraY = 0.3f;
    [SerializeField] Transform cam;
    [SerializeField] Transform cameraHolder;
    [SerializeField] InputActionReference sprintButton;
    [SerializeField] InputActionReference crouchButton;
    [Header("Sound")]
    [SerializeField] float walkVolumeChangeInterval = 0.25f;
    [SerializeField] AudioSource walksound;
    [SerializeField] AudioClip[] walkClips;
    [SerializeField] Vector2 walkVolumeRange = new Vector2(0.4f, 1f);
    [Header("Animation")]
    [SerializeField] Animator animator;

    private CharacterController controller;
    Vector2 moveInput;
    private Vector3 velocity;

    public bool canMove = true;
    public bool IsMoving { get; private set; }
    bool wasMoving;
    bool isCrouch;
    float nextWalkVolumeTime;
    float normalMoveSpeed;

    void OnEnable()
    {
        sprintButton.action.Enable();
        crouchButton.action.Enable();
    }
    void OnDisable()
    {
        sprintButton.action.Disable();
        crouchButton.action.Disable();
        
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (walksound != null)
        {
            walksound.loop = true;
        }
        normalMoveSpeed = moveSpeed;
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }


    void Update()
    {
        if (!isLocalPlayer) return;

        playerMove();
        PlayerCrouch();
        walksoundEffect();
    }


    private void PlayerCrouch()
    {
        if (crouchButton.action.IsPressed()) { isCrouch = true; }
        else { isCrouch = false; }

        float targetHeight = isCrouch ? crouchHeight : standHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, crouchSitSpeed * Time.deltaTime);

        controller.center = new Vector3(0, controller.height / 2f, 0);

        float targetCamY = isCrouch ? crouchCameraY : standCameraY;

        Vector3 pos = cameraHolder.localPosition;
        pos.y = Mathf.Lerp(pos.y, targetCamY, crouchSitSpeed * Time.deltaTime);
        cameraHolder.localPosition = pos;
    }

    private void walksoundEffect()
    {
        IsMoving = moveInput.sqrMagnitude > 0.001f;


        if (walksound != null)
        {
            if (IsMoving && !wasMoving)
            {
                AudioClip clipToPlay = null;
                if (walkClips != null && walkClips.Length > 0)
                {
                    int index = Random.Range(0, walkClips.Length);
                    clipToPlay = walkClips[index];
                }

                if (clipToPlay != null)
                {
                    walksound.clip = clipToPlay;
                    SetRandomWalkVolume();
                    nextWalkVolumeTime = Time.time + walkVolumeChangeInterval;
                    walksound.Play();
                }
            }
            else if (!IsMoving && wasMoving)
                walksound.Stop();
        }
        if (IsMoving && walksound != null && walksound.isPlaying && Time.time >= nextWalkVolumeTime)
        {
            SetRandomWalkVolume();
            nextWalkVolumeTime = Time.time + walkVolumeChangeInterval;
        }
        wasMoving = IsMoving;
    }

    private void playerMove()
    {
        if (!canMove) return;

        Vector3 camForward = Quaternion.Euler(0, cam.eulerAngles.y, 0) * Vector3.forward;
        Vector3 camRight = Quaternion.Euler(0, cam.eulerAngles.y, 0) * Vector3.right;

        Vector3 moveDir = camRight * moveInput.x + camForward * moveInput.y;

        if (sprintButton.action.IsPressed() && !isCrouch && IsMoving) { moveSpeed = sprintSpeed; animator.SetBool("isRuning", true); }
        else if (isCrouch) { moveSpeed = crouchSpeed; animator.SetBool("isRuning", false); }
        else { moveSpeed = normalMoveSpeed; animator.SetBool("isRuning", false); }


        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        Vector3 finalMove = velocity * Time.deltaTime;
        controller.Move(finalMove);
        animator.SetBool("isWalking", IsMoving);
    }



    void SetRandomWalkVolume()
    {
        float min = Mathf.Clamp01(walkVolumeRange.x);
        float max = Mathf.Clamp01(walkVolumeRange.y);
        if (max < min)
        {
            float temp = min;
            min = max;
            max = temp;
        }
        walksound.volume = Random.Range(min, max);
    }


}

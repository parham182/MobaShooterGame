using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] Transform cam;
    [SerializeField] AudioSource walksound;
    [SerializeField] AudioClip[] walkClips;
    [SerializeField] Vector2 walkVolumeRange = new Vector2(0.4f, 1f);
    [SerializeField] float walkVolumeChangeInterval = 0.25f;
    [SerializeField] float gravity = -20f;
    private CharacterController controller;

    Vector2 moveInput;
    private Vector3 velocity;

    public bool canMove = true;
    public bool IsMoving { get; private set; }
    bool wasMoving;
    float nextWalkVolumeTime;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (walksound != null)
        {
            walksound.loop = true;
        }
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        if(!isLocalPlayer) return;
        
        playerMove();
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

        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        Vector3 finalMove = velocity * Time.deltaTime;
        controller.Move(finalMove);
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

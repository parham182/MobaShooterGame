using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRotation : NetworkBehaviour
{
    [SerializeField] private GameObject playerCamera;
    public InputActionReference mouse;
    [SerializeField] float sensitivity = 120f;
    [SerializeField] Transform playerBody;

    [SerializeField] float xClampMin = -90f;
    [SerializeField] float xClampMax = 90f;

    float xRotation;
    float yRotation;

    void OnEnable()
    {
        mouse.action.Enable();
    }

    void OnDisable()
    {
        mouse.action.Disable();
    }

    public override void OnStartLocalPlayer()
    {
        playerCamera.SetActive(true);
    }

    private void Start()
    {
        if (!isLocalPlayer)
        {
            playerCamera.SetActive(false);
        }
        if (playerBody == null)
        {
            playerBody = transform.parent;
        }

        // Initialize from current scene rotation to avoid snapping on first frame.
        xRotation = transform.localEulerAngles.x;
        if (xRotation > 180f)
        {
            xRotation -= 360f;
        }

        if (playerBody != null)
        {
            yRotation = playerBody.localEulerAngles.y;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        LookAround();
    }

    void LookAround()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        Vector2 look = mouse.action.ReadValue<Vector2>();

        float mouseX = look.x * sensitivity * Time.deltaTime;
        float mouseY = look.y * sensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;


        xRotation = Mathf.Clamp(xRotation, xClampMin, xClampMax);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
        {
            playerBody.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }

}

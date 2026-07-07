using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRotation : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Transform playerBody;
    public InputActionReference mouse;

    [Header("Settings")]
    [SerializeField] private float sensitivity = 120f;
    [SerializeField] private float xClampMin = -90f;
    [SerializeField] private float xClampMax = 90f;

    private float xRotation;
    private float yRotation;
    
    bool canRotate;

    void OnEnable()
    {
        if (mouse != null)
            mouse.action.Enable();
    }

    void OnDisable()
    {
        if (mouse != null)
            mouse.action.Disable();
    }

    public override void OnStartLocalPlayer()
    {
        if (playerCamera != null)
            playerCamera.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        canRotate = true;
        if (!isLocalPlayer)
        {
            if (playerCamera != null)
                playerCamera.SetActive(false);

            return;
        }

        if (playerBody == null)
            playerBody = transform.parent;

        // sync initial rotation
        xRotation = transform.localEulerAngles.x;
        if (xRotation > 180f) xRotation -= 360f;

        if (playerBody != null)
            yRotation = playerBody.localEulerAngles.y;
    }

    public void DisableCameraRotation()
    {
        canRotate = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void EnableCameraRotation()
    {
        canRotate = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void LateUpdate()
    {
        if (!isLocalPlayer) return;
        if (mouse == null) return;
        LookAround();
    }

    void LookAround()
    {
        if (!canRotate) return;

        Vector2 look = mouse.action.ReadValue<Vector2>();

        float mouseX = look.x * sensitivity * Time.deltaTime;
        float mouseY = look.y * sensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, xClampMin, xClampMax);

        // camera pitch
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // player yaw
        if (playerBody != null)
            playerBody.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }

}
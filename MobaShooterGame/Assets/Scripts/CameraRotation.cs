using Mirror;
using UnityEngine;

public class CameraRotation : NetworkBehaviour
{
    [SerializeField] private GameObject playerCamera;

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
    }
}

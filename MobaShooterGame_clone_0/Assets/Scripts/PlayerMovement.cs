using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    private void Update()
    {
        if (isLocalPlayer)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            Vector3 playerMovement = new Vector3(h, 0, v);
            transform.position = transform.position + playerMovement;
        }
    }
}

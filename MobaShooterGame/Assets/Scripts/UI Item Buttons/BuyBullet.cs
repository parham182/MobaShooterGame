using Mirror;
using UnityEngine;

public class BuyBullet : MonoBehaviour
{
    public void OnClick()
    {
        NetworkClient.localPlayer.GetComponent<Player>()
        .CmdBuyItem("bullet");
    }
}

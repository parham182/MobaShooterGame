using Mirror;
using UnityEngine;

public class BuyGun : MonoBehaviour
{
    public void OnClick()
    {
        NetworkClient.localPlayer.GetComponent<Player>()
        .CmdBuyItem("AK47");
    }
}

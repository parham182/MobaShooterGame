using Mirror;
using UnityEngine;

public class BuyMedKit : NetworkBehaviour
{
    public void OnClick()
    {
        NetworkClient.localPlayer.GetComponent<Player>()
        .CmdBuyItem("med kit");
    }
}

using Mirror;
using UnityEngine;

public class BuyMedKit : MonoBehaviour
{
    public void OnClick()
    {
        NetworkClient.localPlayer.GetComponent<Player>()
        .CmdBuyItem("med kit");
    }
}

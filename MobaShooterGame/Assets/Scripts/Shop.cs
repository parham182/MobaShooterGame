using Mirror;
using UnityEngine;

public class Shop : NetworkBehaviour
{
    public float hpRegBuff = 10;
    public string shopSide;

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        if (other.TryGetComponent(out Player player))
        {
            if (player.playerSide == shopSide)
            {
                // player.isInShop = true;
                // player.AddRegenBuff(hpRegBuff);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isServer) return;

        if (other.TryGetComponent(out Player player))
        {
            if (player.playerSide == shopSide)
            {
                // player.isInShop = false;
                // player.RemoveRegenBuff(hpRegBuff);
            }
        }
    }
}
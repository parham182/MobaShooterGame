using UnityEngine;

public class Shop : MonoBehaviour
{
    public int shopSide;
    public bool playerIsInShop;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.TryGetComponent(out Player player))
        {
            if (player.playerSide == shopSide)
            {
                playerIsInShop = true;
            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.TryGetComponent(out Player player))
        {
            playerIsInShop = false;
        }
    }
}

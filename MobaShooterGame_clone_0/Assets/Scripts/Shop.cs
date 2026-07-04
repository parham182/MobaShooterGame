using UnityEngine;

public class Shop : MonoBehaviour
{
    public int shopSide;

    private void OnTriggerEnter(Collider collider) {
        if (collider.TryGetComponent(out Player player))
        {
            if (player.playerSide == shopSide)
            {
                player.isInSop = true;
            }
        }
    }

    private void OnTriggerExit(Collider collider) {
        if (collider.TryGetComponent(out Player player))
        {
            player.isInSop = false;
        }
    }
}

using Mirror;
using UnityEngine;

public class CurrencyManager : NetworkBehaviour
{
    [SyncVar]
    public int bluePlayerGold;
    [SyncVar]
    public int redPlayerGold;

    public static CurrencyManager instance;

    private void Awake()
    {
        instance = this;
    }

    [Server]
    public void AddGold(int gold, string side)
    {
        if (side == "red")
        {
            redPlayerGold += gold;
        } else if (side == "blue")
        {
            bluePlayerGold += gold;
        }
    }
}

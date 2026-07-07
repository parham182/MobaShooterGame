using Mirror;
using TMPro;
using UnityEngine;

public class CurrencyManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnBlueGoldChanged))]
    public int bluePlayerGold;

    [SyncVar(hook = nameof(OnRedGoldChanged))]
    public int redPlayerGold;

    [SerializeField] private TMP_Text goldText;
    public GameObject shopMenu;

    public static CurrencyManager instance;

    private void Awake()
    {
        instance = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        UpdateUI();
    }

    [Server]
    public void AddGold(int gold, string side)
    {
        if (side == "blue")
            bluePlayerGold += gold;
        else
            redPlayerGold += gold;
    }

    [Server]
    public void RemoveGold(int gold, string side)
    {
        if (side == "blue")
            bluePlayerGold -= gold;
        else
            redPlayerGold -= gold;
    }

    void OnBlueGoldChanged(int oldValue, int newValue)
    {
        UpdateUI();
    }

    void OnRedGoldChanged(int oldValue, int newValue)
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (NetworkClient.localPlayer == null)
            return;

        Player player = NetworkClient.localPlayer.GetComponent<Player>();

        if (player == null)
            return;

        if (player.playerSide == "blue")
            goldText.text = bluePlayerGold.ToString();
        else
            goldText.text = redPlayerGold.ToString();
    }

    public int GetPrice(string itemName)
    {
        switch(itemName)
        {
            case "med kit": return 10;
            case "bullet": return 20;
            case "AK47": return 200;
        }
        return 999999;
    }

    public int GetGold(string playerSide)
    {
        if (playerSide == "blue")
            return bluePlayerGold;
        else
            return redPlayerGold;
    }
}

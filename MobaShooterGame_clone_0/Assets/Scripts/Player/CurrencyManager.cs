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
}
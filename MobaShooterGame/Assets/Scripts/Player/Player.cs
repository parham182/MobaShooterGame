using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour, IDamageable
{
    public float maxHealth = 100;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public float currentHealth;

    [SyncVar(hook = nameof(OnSideChanged))]
    public string playerSide;

    public Gun gun;
    [SyncVar]
    public bool isInShop = false;

    [SerializeField] InputActionReference openShopRef;

    private void OnEnable()
    {
        openShopRef.action.performed += TryToOpenShop;
    }

    private void OnDisable()
    {
        openShopRef.action.performed -= TryToOpenShop;
    }

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
    }

    public override void OnStartClient()
    {
        if (gun != null)
            gun.side = playerSide;
    }

    [Command]
    public void CmdSetInShop(bool value)
    {
        isInShop = value;
    }

    [Server]
    public void TakeDamage(float damage, damageableType attackerType, string attackerSide)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Respawn();
        }
    }

    [Server]
    public void Respawn()
    {
        if (playerSide == "red") 
            GetComponent<NetworkTransformReliable>()
            .RpcTeleport(SpawnManager.instance.redTeamSpawnPoint.position);
        else
            GetComponent<NetworkTransformReliable>()
            .RpcTeleport(SpawnManager.instance.blueTeamSpawnPoint.position);

        currentHealth = maxHealth;
    }

    private void TryToOpenShop(InputAction.CallbackContext value)
    {
        if (!isLocalPlayer || !isInShop) return;

        if (CurrencyManager.instance.shopMenu.activeSelf)
        {
            CurrencyManager.instance.shopMenu.SetActive(false);
            // enable movements
            GetComponent<PlayerMovement>().canMove = true;
            // enable camera rotation
        } else
        {
            CurrencyManager.instance.shopMenu.SetActive(true);
            // disable movements
            GetComponent<PlayerMovement>().canMove = false;
            // disable camera rotation
        }
    }

    void OnHealthChanged(float oldValue, float newValue)
    {
        if (!isLocalPlayer) return;

        Healthbar.instance.slider.maxValue = maxHealth;
        Healthbar.instance.slider.value = newValue;
        Healthbar.instance.healthText.text = $"{newValue}/{maxHealth}";
    }

    void OnSideChanged(string oldValue, string newValue)
    {
        if (gun != null)
            gun.side = newValue;
    }

    [Command]
    public void CmdBuyItem(string itemName)
    {
        if (!isInShop)
            return;

        int price = CurrencyManager.instance.GetPrice(itemName);

        if (CurrencyManager.instance.GetGold(playerSide) < price)
            return;

        CurrencyManager.instance.RemoveGold(price, playerSide);

        GiveItem(itemName);
    }

    public string DamageableSide() => playerSide;

    public damageableType GetDamageableType() => damageableType.Player;

    public Vector3 GetPosision() => transform.position;

    [Server]
    void GiveItem(string itemName)
    {
        switch(itemName)
        {
            case "med kit":
                {
                    currentHealth += 50;
                    if (currentHealth > maxHealth) currentHealth = maxHealth;
                }
                break;

            case "AK47":
                print("Player got AK47");
                break;
        }
    }
}

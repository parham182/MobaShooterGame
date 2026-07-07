using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : NetworkBehaviour, IDamageable
{
    public float maxHealth = 100;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public float currentHealth;

    [SyncVar(hook = nameof(OnSideChanged))]
    public string playerSide;

    [SyncVar(hook = nameof(OnGunChanged))]
    private string currentGun = "";

    public Gun gun;

    public GameObject coltGun;
    public GameObject ak47Gun;

    [SyncVar]
    public bool isInShop = false;

    public Renderer playerModelRenderer;
    public Slider globalHealthbarSlider;

    [SerializeField] InputActionReference openShopRef;

    private void OnEnable()
    {
        openShopRef.action.performed += TryToOpenShop;
    }

    private void OnDisable()
    {
        openShopRef.action.performed -= TryToOpenShop;
    }

    private void Start()
    {
        if (isLocalPlayer)
            playerModelRenderer.enabled = false;
    }

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
        GiveItem("colt");
    }

    [Command]
    public void CmdSetInShop(bool value)
    {
        isInShop = value;
    }

    [Server]
    public void TakeDamage(float damage, damageableType attackerType, string attackerSide)
    {
        if (attackerSide == playerSide)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            if (attackerType == damageableType.Player)
                CurrencyManager.instance.AddGold(200, attackerSide);

            currentHealth = 0;
            Respawn();
        }
    }

    [Server]
    public void Respawn()
    {
        if (playerSide == "red")
        {
            GetComponent<NetworkTransformReliable>()
                .RpcTeleport(SpawnManager.instance.redTeamSpawnPoint.position);
        }
        else
        {
            GetComponent<NetworkTransformReliable>()
                .RpcTeleport(SpawnManager.instance.blueTeamSpawnPoint.position);
        }

        GiveItem("bullet");
        currentHealth = maxHealth;
    }

    void TryToOpenShop(InputAction.CallbackContext value)
    {
        if (!isLocalPlayer || !isInShop)
            return;

        bool opened = CurrencyManager.instance.shopMenu.activeSelf;

        CurrencyManager.instance.shopMenu.SetActive(!opened);

        GetComponent<PlayerMovement>().canMove = opened;

        if (opened)
            GetComponent<CameraRotation>().EnableCameraRotation();
        else
            GetComponent<CameraRotation>().DisableCameraRotation();
    }

    void OnHealthChanged(float oldValue, float newValue)
    {
        globalHealthbarSlider.maxValue = maxHealth;
        globalHealthbarSlider.value = newValue;

        if (!isLocalPlayer)
            return;

        Healthbar.instance.slider.maxValue = maxHealth;
        Healthbar.instance.slider.value = newValue;
        Healthbar.instance.healthText.text = $"{newValue}/{maxHealth}";
    }

    void OnSideChanged(string oldValue, string newValue)
    {
        if (gun != null)
            gun.side = newValue;
    }

    void OnGunChanged(string oldValue, string newValue)
    {
        coltGun.SetActive(false);
        ak47Gun.SetActive(false);

        switch (newValue)
        {
            case "colt":
                coltGun.SetActive(true);
                gun = coltGun.GetComponent<Gun>();
                break;

            case "AK47":
                ak47Gun.SetActive(true);
                gun = ak47Gun.GetComponent<Gun>();
                break;
        }

        if (gun != null)
            gun.side = playerSide;
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
        switch (itemName)
        {
            case "med kit":
                currentHealth = Mathf.Min(currentHealth + 80, maxHealth);
                break;

            case "bullet":
                gun.RefreshBullet();
                break;

            case "colt":
                currentGun = "colt";
                break;

            case "AK47":
                currentGun = "AK47";
                break;
        }
    }
}
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour, IDamageable
{
    public float maxHealth = 100;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public float currentHealth;

    [SyncVar(hook = nameof(OnSideChanged))]
    public string playerSide;

    [SyncVar(hook = nameof(OnGunChanged))]
    public Gun gun;
    public GameObject AK47Prefab;
    public GameObject coltPrefab;
    public Transform gunHolder;
    [SyncVar]
    public bool isInShop = false;
    public Renderer playerModelRenderer;
    // public GameObject fakeGun;

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
        if (!isLocalPlayer) return;

        playerModelRenderer.enabled = false;
        // Destroy(fakeGun);
    }

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
        GiveItem("colt");
    }

    // public override void OnStartClient() { }

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
            GetComponent<CameraRotation>().EnableCameraRotation();
        } else
        {
            CurrencyManager.instance.shopMenu.SetActive(true);
            // disable movements
            GetComponent<PlayerMovement>().canMove = false;
            // disable camera rotation
            GetComponent<CameraRotation>().DisableCameraRotation();
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
        print("u buyed Item " + playerSide);
        switch(itemName)
        {
            case "med kit":
                {
                    currentHealth += 50;
                    if (currentHealth > maxHealth) currentHealth = maxHealth;
                }
                break;

            case "AK47":
                {
                    // destroy current gun
                    NetworkServer.Destroy(gun.gameObject);
                    // spawn new gun
                    GameObject newGunObj = Instantiate(AK47Prefab, gunHolder.position, gunHolder.rotation);
                    Gun newGun = newGunObj.GetComponent<Gun>();
                    newGun.side = playerSide;
                    NetworkServer.Spawn(newGunObj, connectionToClient);
                    // set it as current gun
                    gun = newGun;
                }
                break;
            case "colt":
                {
                    // spawn new gun
                    GameObject newGunObj = Instantiate(coltPrefab, gunHolder.position, gunHolder.rotation);
                    Gun newGun = newGunObj.GetComponent<Gun>();
                    newGun.side = playerSide;
                    NetworkServer.Spawn(newGunObj, connectionToClient);
                    // set it as current gun
                    gun = newGun;
                }
                break;
        }
    }

    void OnGunChanged(Gun oldGun, Gun newGun)
    {
        if (newGun == null)
            return;

        newGun.transform.SetParent(gunHolder, false);
        // newGun.transform.localPosition = Vector3.zero;
        // newGun.transform.localRotation = Quaternion.identity;
    }
}

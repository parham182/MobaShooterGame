using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : NetworkBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] float fireRate = 10f;
    public float fullMagazine = 30;
    
    [SyncVar(hook = nameof(UpdateUI))]
    public float bulletsInMagazine;

    [SyncVar(hook = nameof(UpdateUI))]
    public float bulletAmount;

    public float maxBulletAmount;

    public float damage = 10f;
    public float range = 100f;
    public float spread;
    public string side;

    [SerializeField] Animator animator;

    float nextFireTime;

    [Header("Raycast")]
    [SerializeField] LayerMask impactLayer;

    [Header("Effects")]
    public GameObject firePoint;
    public GameObject muzzleFlash;
    public GameObject ImpactEffect;
    [SerializeField] AudioClip fireSound;
    [SerializeField] AudioSource fireAudioSource;

    [Header("Reload")]
    [SyncVar]
    bool isReloading;

    [Header("Input")]
    [SerializeField] InputActionReference fireRef;
    [SerializeField] InputActionReference reloadRef;

    private void OnEnable()
    {
        fireRef.action.Enable();
        reloadRef.action.performed += TryToReload;
    }

    private void OnDisable()
    {
        fireRef.action.Disable();
        reloadRef.action.performed -= TryToReload;
    }

    public override void OnStartAuthority()
    {
        UpdateUI(0,0);
    }

    void Update()
    {
        if (!isOwned)
            return;

        if (fireRef.action.IsPressed() && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + (1f / fireRate);
            Fire();
        }
    }

    void TryToReload(InputAction.CallbackContext value)
    {
        if (bulletsInMagazine < fullMagazine)
            if (bulletAmount > 0)
                Reload();
    }

    void Fire()
    {
        if (isReloading)
            return;

        if (bulletsInMagazine <= 0)
        {
            if (bulletAmount > 0)
                Reload();

            return;
        }

        animator.SetTrigger("Fire");

        Vector3 direction = Camera.main.transform.forward;

        direction += Camera.main.transform.right *
                     Random.Range(-spread, spread);
        direction += Camera.main.transform.up *
                     Random.Range(-spread, spread);

        direction.Normalize();

        CmdShoot(Camera.main.transform.position, direction);
    }

    [Command]
    void CmdShoot(Vector3 origin, Vector3 direction)
    {
        if (isReloading)
            return;

        if (bulletsInMagazine <= 0)
            return;

        bulletsInMagazine--;

        RpcPlayMuzzleFlash();

        RaycastHit hit;

        if (Physics.Raycast(
            origin,
            direction,
            out hit,
            range))
        {
            if (hit.collider.TryGetComponent(out IDamageable damageable))
            {

                if (damageable.GetDamageableType() == damageableType.Building)
                {
                    if (damageable.DamageableSide() != side)
                    {
                        damageable.TakeDamage(
                            damage,
                            damageableType.Player,
                            side);
                    }
                }
                else
                {
                    damageable.TakeDamage(
                        damage,
                        damageableType.Player,
                        side);
                }
            }

            RpcSpawnEffect(
                hit.point,
                Quaternion.LookRotation(hit.normal));
        }
    }

    [ClientRpc]
    void RpcPlayMuzzleFlash()
    {
        GameObject obj = Instantiate(
            muzzleFlash,
            firePoint.transform.position,
            firePoint.transform.rotation,
            firePoint.transform);

        fireAudioSource.pitch = Random.Range(0.9f, 1.1f);
        fireAudioSource.PlayOneShot(fireSound);

        Destroy(obj,2f);
    }

    [ClientRpc]
    void RpcSpawnEffect(Vector3 position, Quaternion rotation)
    {
        GameObject obj = Instantiate(
            ImpactEffect,
            position,
            rotation);


        Destroy(obj,2f);
    }

    [Server]
    public void RefreshBullet()
    {
        bulletAmount = maxBulletAmount;
        bulletsInMagazine = fullMagazine;
    }

    void UpdateUI(float oldValue,float newValue)
    {
        if (!isOwned)
            return;


        if (Healthbar.instance != null)
        {
            Healthbar.instance.bulletStatusText.text =
                $"{bulletsInMagazine}/{bulletAmount}";
        }
    }

    void Reload()
    {
        if (!isOwned)
            return;


        if (isReloading)
            return;


        CmdStartReload();


        animator.SetTrigger("Reload");


        if (Healthbar.instance != null)
            Healthbar.instance.bulletStatusText.text =
                "Reloading...";
    }

    [Command]
    void CmdStartReload()
    {
        if (isReloading)
            return;


        if (bulletAmount <= 0)
            return;


        if (bulletsInMagazine >= fullMagazine)
            return;


        isReloading = true;
    }

    // Animation Event
    public void EndOfReloading()
    {
        if (!isOwned)
            return;


        CmdFinishReload();
    }

    [Command]
    void CmdFinishReload()
    {
        if (!isReloading)
            return;


        float needed =
            fullMagazine - bulletsInMagazine;



        if (bulletAmount >= needed)
        {
            bulletAmount -= needed;
            bulletsInMagazine += needed;
        }
        else
        {
            bulletsInMagazine += bulletAmount;
            bulletAmount = 0;
        }
        isReloading = false;
    }
}
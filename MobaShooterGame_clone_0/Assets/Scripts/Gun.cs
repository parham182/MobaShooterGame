using System.Collections;
using Mirror;

using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : NetworkBehaviour
{
    [SerializeField] float fireRate = 10f; 
    [SerializeField] float magezineSize = 7f;
    [SerializeField] Animator animator;
    float nextFireTime;
    float fullMagezine;

    [SerializeField] LayerMask impactlayer;
    public float damage = 10f;
    public float range = 100f;
    public string side;
    public float spread;

    [Header("effects")]
    public GameObject firePoint;
    public GameObject muzzleFlash;
    public GameObject ImpactEffect;
    public GameObject bloodImpactEffect;

    [SerializeField] InputActionReference fireRef;

    bool canShoot;

    private void OnEnable()
    {
        fireRef.action.Enable();
    }

    private void OnDisable()
    {
        fireRef.action.Disable();
    }

    void Start()
    {
        fullMagezine = magezineSize;
    }

    void Update()
    {
        if (fireRef.action.IsPressed() && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + (1f / fireRate);
            Fire();

        }
    }

    private void Fire()
    {
        if (!isOwned) return;

        animator.SetTrigger("Fire");
        print("shoot");

        Vector3 direction = Camera.main.transform.forward;
        direction += Camera.main.transform.right * Random.Range(-spread, spread);
        direction += Camera.main.transform.up * Random.Range(-spread, spread);
        direction.Normalize();

        CmdShoot(Camera.main.transform.position, direction);
    }

    [Command]
    void CmdShoot(Vector3 origin, Vector3 direction)
    {
        RpcPlayMuzzleFlash();
        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, range, impactlayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                if (damageable.GetDamageableType() == damageableType.Building)
                {
                    if (damageable.DamageableSide() != side) damageable.TakeDamage(damage, damageableType.Player, side);
                }
                else damageable.TakeDamage(damage, damageableType.Player, side);
            }

            SpawnEffect(hit.point, Quaternion.LookRotation(hit.normal));
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

        Destroy(obj, 2f);
    }

    [Server]
    void SpawnEffect(Vector3 position, Quaternion rotation)
    {
        RpcSpawnEffect(position, rotation);
    }

    [ClientRpc]
    void RpcSpawnEffect(Vector3 position, Quaternion rotation)
    {
        GameObject obj = Instantiate(ImpactEffect, position, rotation);
        Destroy(obj, 2f);
    }
}

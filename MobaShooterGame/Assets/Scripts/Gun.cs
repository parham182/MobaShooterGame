using System.Collections;
using Mirror;

using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : NetworkBehaviour
{
    [SerializeField] GameObject Test;
    [SerializeField] float fireRate = 10f; 
    [SerializeField] float magezineSize = 7f;
    float nextFireTime;
    float fullMagezine;


    [SerializeField] LayerMask impactlayer;
    public float damage = 10f;
    public float range = 100f;
    public string side;
    float spread = 0.05f;

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
        if (!isLocalPlayer) return;

        SpawnEffect(muzzleFlash, firePoint.transform.position, Quaternion.identity, firePoint.transform);

        Vector3 direction = Camera.main.transform.forward;
        direction += Camera.main.transform.right * Random.Range(-spread, spread);
        direction += Camera.main.transform.up * Random.Range(-spread, spread);
        direction.Normalize();

        CmdShoot(Camera.main.transform.position, direction);
    }

    [Command]
    void CmdShoot(Vector3 origin, Vector3 direction)
    {
        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, range, impactlayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                if (damageable.GetDamageableType() == damageableType.Building)
                {
                    if (damageable.DamageableSide() != side) damageable.TakeDamage(damage);
                }
                else damageable.TakeDamage(damage);
            }

            Instantiate(Test, hit.point, Quaternion.identity);
            SpawnEffect(ImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }

    [Server]
    private void SpawnEffect(GameObject effectObject, Vector3 position, Quaternion rotation)
    {
        // GameObject impactObject = Instantiate(effectObject, position, rotation);
        // NetworkServer.Spawn(impactObject);
        // StartCoroutine(DespawnAfter(impactObject, 2f));
    }

    [Server]
    private void SpawnEffect(GameObject effectObject, Vector3 position, Quaternion rotation, Transform parent)
    {
        // GameObject impactObject = Instantiate(effectObject, position, rotation, parent);
        // NetworkServer.Spawn(impactObject);
        // StartCoroutine(DespawnAfter(impactObject, 2f));
    }

    IEnumerator DespawnAfter(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);

        NetworkServer.UnSpawn(obj);
        Destroy(obj);
    }
}

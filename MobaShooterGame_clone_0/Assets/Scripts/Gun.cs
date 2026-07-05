using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : NetworkBehaviour
{
    [SerializeField] LayerMask impactlayer;
    public float damage = 10f;
    public float range = 100f;
    public string side;

    [Header("effects")]
    public GameObject firePoint;
    public GameObject muzzleFlash;
    public GameObject ImpactEffect;
    public GameObject bloodImpactEffect;

    [SerializeField] InputActionReference fireRef;

    private void OnEnable()
    {
        fireRef.action.performed += Fire;
    }

    private void OnDisable()
    {
        fireRef.action.performed -= Fire;
    }

    private void Fire(InputAction.CallbackContext value)
    {
        if (!isLocalPlayer) return;

        SpawnEffect(muzzleFlash, firePoint.transform.position, Quaternion.identity, firePoint.transform);

        CmdShoot(Camera.main.transform.position, Camera.main.transform.forward);
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
                } else damageable.TakeDamage(damage);
            }

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

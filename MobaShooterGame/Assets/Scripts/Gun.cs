using Mirror;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : NetworkBehaviour
{
    [SerializeField] LayerMask impactlayer;
    public float damage = 10f;
    public float range = 100f;
    public int side = 0;

    [Header("effects")]
    public GameObject firePoint;
    public ParticleSystem muzzleFlash;
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
        print("Shoot");

        Instantiate(muzzleFlash,firePoint.transform.position, quaternion.identity, firePoint.transform);

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

        Instantiate(ImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));

            // GameObject impactObject = Instantiate(ImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            // Destroy(impactObject, 2f);
        }
    }
}

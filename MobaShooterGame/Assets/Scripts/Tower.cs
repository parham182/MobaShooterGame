using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Tower : NetworkBehaviour, IDamageable
{
    [SerializeField] float attackInterval;
    [SerializeField] float attackRange;
    [SerializeField] float damage;
    [SerializeField] float playerDamageMultiplier;
    [SerializeField] float noCreepDamageReduction;

    [SerializeField] GameObject TowerGun;
    [SerializeField] Transform firePoint;
    public GameObject muzzleFlash;

    public string towerSide;
    private IDamageable target;
    public Slider globalHealthbarSlider;

    private float timer;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public float health;

    public override void OnStartServer()
    {
        SpawnManager.instance.AddDamageable(this);
    }

    void Start()
    {
        globalHealthbarSlider.maxValue = health;
        globalHealthbarSlider.value = health;
    }

    [ServerCallback]
    private void Update()
    {
        timer += Time.deltaTime;
        // find target
        float closestTargetDistance = float.MaxValue;
        target = null;

        foreach (IDamageable t in SpawnManager.instance.targets)
        {
            if (t.DamageableSide() != towerSide)
            {
                float distance = Vector3.Distance(transform.position, t.GetPosision());
                if (distance <= closestTargetDistance)
                {
                    closestTargetDistance = distance;
                    target = t;
                }
            }
        }

        if (target != null)
        {
            Vector3 dir = target.GetPosision() - TowerGun.transform.position;
            dir.y = 0f;

            if (dir != Vector3.zero) TowerGun.transform.rotation =
                Quaternion.LookRotation(dir) * Quaternion.Euler(0, 90f, 0);

            if (timer >= attackInterval)
            {
                float distance = Vector3.Distance(transform.position, target.GetPosision());
                if (distance <= attackRange) // attack the target
                {
                    timer = 0;
                    RpcPlayMuzzleFlash();
                    float finalDamage = 
                        target.GetDamageableType() == damageableType.Player
                        ? damage * playerDamageMultiplier
                        : damage;
                            
                    target.TakeDamage(finalDamage, damageableType.Building, towerSide);
                }
            }
        }
    }

    // ---------------- DAMAGE ----------------

    [Server]
    public void TakeDamage(float damage, damageableType attackerType, string attackerSide)
    {
        bool hasCreep = false;

        if (target != null && target.GetDamageableType() == damageableType.Creep) hasCreep = true;

        float finalDamage = hasCreep ? damage : damage * noCreepDamageReduction;

        health -= finalDamage;

        if (health <= 0)
        {
            NetworkServer.Destroy(gameObject);
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

    void OnHealthChanged(float oldValue, float newValue)
    {
        globalHealthbarSlider.value = newValue;
    }

    // ---------------- INTERFACE ----------------

    public string DamageableSide() => towerSide;

    public damageableType GetDamageableType() => damageableType.Building;

    public Vector3 GetPosision() => transform.position;
}

using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Tower : NetworkBehaviour, IDamageable
{
    [SerializeField] float attackInterval;
    [SerializeField] float attackRange;
    [SerializeField] float damage;
    [SerializeField] float playerDamageMultiplier;
    [SerializeField] float noCreepDamageReduction;

    [SerializeField] GameObject TowerGun;
    [SerializeField] Transform attackPoint;

    public string towerSide;
    private IDamageable target;

    private float timer;

    [SyncVar]
    public float health = 100;

    public override void OnStartServer()
    {
        health = 100;
        SpawnManager.instance.AddDamageable(this);
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

        if (target != null && timer >= attackInterval)
        {
            timer = 0;
            float distance = Vector3.Distance(transform.position, target.GetPosision());
            if (distance <= attackRange) // attack the target
            {
                Vector3 dir = target.GetPosision() - TowerGun.transform.position;
                dir.y = 0f;

                if (dir != Vector3.zero)
                {
                    TowerGun.transform.rotation =
                        Quaternion.LookRotation(dir) * Quaternion.Euler(0, 90f, 0);
                }
                Vector3 rayDir = Quaternion.Euler(0, -90f, 0) * TowerGun.transform.forward;

                Debug.DrawRay(attackPoint.position, rayDir, Color.red);
                if (Physics.Raycast(attackPoint.position, rayDir, out RaycastHit hit))
                {
                    if (hit.collider.TryGetComponent(out IDamageable dmg))
                    {
                        float finalDamage = 
                            dmg.GetDamageableType() == damageableType.Player
                            ? damage * playerDamageMultiplier
                            : damage;
                        
                        dmg.TakeDamage(finalDamage);
                    }
                }
            }
        }
    }

    // ---------------- DAMAGE ----------------

    [Server]
    public void TakeDamage(float damage)
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

    // ---------------- INTERFACE ----------------

    public string DamageableSide() => towerSide;

    public damageableType GetDamageableType() => damageableType.Building;

    public Vector3 GetPosision() => transform.position;
}

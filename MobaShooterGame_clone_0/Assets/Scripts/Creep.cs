using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class Creep : NetworkBehaviour, IDamageable
{
    [SyncVar]
    public string creepSide;
    [SyncVar]
    public float health;

    [Header("Creep Stats")]
    [SerializeField] private int bountyGold;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackInterval;
    [SerializeField] private float damage;
    [SerializeField] private float towerDamageMultiplier;
    [SerializeField] private Transform firePoint;
    [SerializeField] Animator animator;
    [SerializeField] NavMeshAgent navMeshAgent;

    private IDamageable target;
    private float timer = 0;

    [ServerCallback]
    private void Update()
    {
        timer += Time.deltaTime;
        // find target
        float closestTargetDistance = float.MaxValue;
        target = null;

        foreach (IDamageable t in SpawnManager.instance.targets)
        {
            if (t.DamageableSide() != creepSide)
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
            float distance = Vector3.Distance(transform.position, target.GetPosision());
            if (distance <= attackRange) // attack the target
            {
                navMeshAgent.isStopped = true;

                if (timer >= attackInterval)
                {
                    timer = 0;
                    Vector3 dir = (target.GetPosision() - transform.position).normalized;

                    if (Physics.Raycast(firePoint.position, dir, out RaycastHit hit))
                    {
                        if (hit.collider.TryGetComponent(out IDamageable dmg))
                        {
                            float finalDamage =
                                dmg.GetDamageableType() == damageableType.Building
                                ? damage * towerDamageMultiplier
                                : damage;

                            dmg.TakeDamage(finalDamage, damageableType.Creep, creepSide);
                        }
                    }
                }
            }
            else // go closer (target its not in attack range)
            {
                navMeshAgent.SetDestination(target.GetPosision());
                navMeshAgent.isStopped = false;
            }
        }
    }

    public string DamageableSide()
    {
        return creepSide;
    }

    public damageableType GetDamageableType()
    {
        return damageableType.Creep;
    }

    public Vector3 GetPosision()
    {
        return transform.position;
    }

    [Server]
    public void TakeDamage(float damage, damageableType attackerType, string attackerSide)
    {
        health -= damage;
        if (health <= 0)
        {
            if (attackerType == damageableType.Player && attackerSide != creepSide)
            {
                CurrencyManager.instance.AddGold(bountyGold, attackerSide);
                print(attackerSide);
            }
            health = 0;
            SpawnManager.instance.RemoveDamageable(this);
            NetworkServer.Destroy(gameObject);
        }
    }
}

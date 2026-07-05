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
    [SerializeField] private float attackRange;
    [SerializeField] private float attackInterval;
    [SerializeField] private float damage;
    [SerializeField] private float towerDamageMultiplier;
    [SerializeField] private Transform firePoint;

    [Header("Creep Movement")]
    [SerializeField] Transform moveTargetBlueTeam;
    [SerializeField] Transform moveTargetRedTeam;
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] Animator animator;
    NavMeshAgent navMeshAgent;

    private IDamageable target;
    private float timer = 0;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

    }

    [ServerCallback]
    private void Update()
    {
        creepMovement();

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

                            dmg.TakeDamage(finalDamage);
                            moveSpeed = 0;
                        }
                    }
                }
            }
            else // go closer (target its not in attack range)
            {
                moveSpeed = 10;
            }
        }
    }

    private void creepMovement()
    {
        navMeshAgent.speed = moveSpeed;
        if (creepSide == "blue")
        {
            navMeshAgent.SetDestination(moveTargetBlueTeam.transform.position);
        }
        else if (creepSide == "red")
        {
            navMeshAgent.SetDestination(moveTargetRedTeam.transform.position);
        }
        if (moveSpeed > 0)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
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
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            SpawnManager.instance.RemoveDamageable(this);
            NetworkServer.Destroy(gameObject);
        }
    }
}

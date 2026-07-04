using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Tower : NetworkBehaviour, IDamageable
{
    [SerializeField] float attackInterval;
    [SerializeField] float damage;
    [SerializeField] float playerDamageMultiplier;
    [SerializeField] float noCreepDamageReduction;
    [SerializeField] GameObject TowerGun;
    [SerializeField] Transform attackPoint;
    public int towerSide;
    private List<IDamageable> targetsInRange = new List<IDamageable>();
    private float timer;

    [SyncVar]
    public float health = 100;
    public bool isTeamPlayerUnderTower;

    private void Update()
    {
        if (!isServer) return;

        timer += Time.deltaTime;

        if (targetsInRange.Count > 0)
        {
            float closestTargetDistance = float.MaxValue;
            IDamageable closestTarget = null;

            foreach(IDamageable target in targetsInRange)
            {
                float distance = Vector3.Distance(transform.position, target.GetPosision());
                if (distance < closestTargetDistance)
                {
                    closestTargetDistance = distance;
                    closestTarget = target;
                }
            }

            // look at target
            Vector3 lookDir = TowerGun.transform.position - closestTarget.GetPosision();
            float angle = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg - 90f;
            TowerGun.transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 direction = (closestTarget.GetPosision() - TowerGun.transform.position).normalized;

            Debug.DrawRay(attackPoint.position, direction * 100f, Color.red);

            if (timer >= attackInterval)
            {
                // attack
                // muzzleFlash.Play()
                timer = 0;

                RaycastHit hit;
                if (Physics.Raycast(attackPoint.position, direction, out hit))
                {
                    if (hit.collider.TryGetComponent(out IDamageable damageable))
                    {
                        if (damageable.GetDamageableType() == damageableType.Player)
                        {
                            damageable.TakeDamage(damage * playerDamageMultiplier);
                        } else damageable.TakeDamage(damage);
                    }

                    // GameObject impactObject = Instantiate(ImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    // Destroy(impactObject, 2f);
                }
            }
        }
    }

    public int DamageableSide()
    {
        return towerSide;
    }

    public void TakeDamage(float damage)
    {
        bool isCreenInRange = false;
        foreach(IDamageable target in targetsInRange)
        {
            if (target.GetDamageableType() == damageableType.Creen)
            {
                isCreenInRange = true;
                break;
            }
        }

        float finalDamage = isCreenInRange ? damage : damage * noCreepDamageReduction;
        health -= finalDamage;
        
        if (health <= 0) Destroy(gameObject);
    }

    public damageableType GetDamageableType()
    {
        return damageableType.Building;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            if (damageable.DamageableSide() != towerSide)
            {
                targetsInRange.Add(damageable);
                isTeamPlayerUnderTower = false;
            }
            else
            {
                isTeamPlayerUnderTower = true;
            }
        }

    }

    private void OnTriggerExit(Collider other) {
        if (other.TryGetComponent(out IDamageable damageable))
            targetsInRange.Remove(damageable);
    }

    public Vector3 GetPosision()
    {
        return new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }
}

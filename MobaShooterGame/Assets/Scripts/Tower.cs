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
    [SerializeField] float armorBuff = 5f;

    public string towerSide;

    private readonly List<IDamageable> targetsInRange = new();
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

        if (targetsInRange.Count == 0)
            return;

        IDamageable closest = null;
        float bestDist = float.MaxValue;

        foreach (var t in targetsInRange)
        {
            if (t == null) continue;

            float d = Vector3.Distance(transform.position, t.GetPosision());
            if (d < bestDist)
            {
                bestDist = d;
                closest = t;
            }
        }

        if (closest == null)
            return;

        Vector3 dir = (closest.GetPosision() - attackPoint.position).normalized;

        TowerGun.transform.rotation = Quaternion.LookRotation(dir);

        if (timer < attackInterval)
            return;

        timer = 0f;

        if (Physics.Raycast(attackPoint.position, dir, out RaycastHit hit))
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

    // ---------------- DAMAGE ----------------

    [Server]
    public void TakeDamage(float damage)
    {
        bool hasCreep = false;

        foreach (var t in targetsInRange)
        {
            if (t != null && t.GetDamageableType() == damageableType.Creep)
            {
                hasCreep = true;
                break;
            }
        }

        float finalDamage = hasCreep ? damage : damage * noCreepDamageReduction;

        health -= finalDamage;

        if (health <= 0)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    // ---------------- TRIGGERS ----------------

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        if (other.TryGetComponent(out IDamageable dmg))
        {
            if (dmg.DamageableSide() != towerSide)
            {
                targetsInRange.Add(dmg);
            }
        }

        if (other.TryGetComponent(out Player player))
        {
            if (player.playerSide == towerSide)
            {
                // player.AddArmorBuff(armorBuff);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isServer) return;

        if (other.TryGetComponent(out IDamageable dmg))
        {
            targetsInRange.Remove(dmg);
        }

        if (other.TryGetComponent(out Player player))
        {
            if (player.playerSide == towerSide)
            {
                // player.RemoveArmorBuff(armorBuff);
            }
        }
    }

    // ---------------- INTERFACE ----------------

    public string DamageableSide() => towerSide;

    public damageableType GetDamageableType() => damageableType.Building;

    public Vector3 GetPosision() => transform.position;
}
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Creep : NetworkBehaviour, IDamageable
{
    [SyncVar]
    public string creepSide;
    [SyncVar(hook = nameof(OnHealthChanged))]
    public float health;

    [Header("Creep Stats")]
    [SerializeField] private int bountyGold;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackInterval;
    [SerializeField] private float damage;
    [SerializeField] private float towerDamageMultiplier;
    [SerializeField] private float spread;
    [SerializeField] private Transform firePoint;
    [SerializeField] Animator animator;
    [SerializeField] NavMeshAgent navMeshAgent;
    [SerializeField] GameObject creepModel;

    [Header("Footstep")]
    [SerializeField] AudioClip dialogSound;
    [SerializeField] AudioClip fireSound;
    [SerializeField] AudioSource fireAudioSource;
    [SerializeField] AudioClip[] footstepSounds;
    [SerializeField] AudioSource walkAudioSource;
    [SerializeField] AudioSource dialogAudioSource;
    [SerializeField] float footstepInterval = 0.4f;

    private float footstepTimer;

    public GameObject muzzleFlash;
    public GameObject ImpactEffect;
    public Slider globalHealthbarSlider;

    private IDamageable target;
    private float timer = 0;

    void Start()
    {
        globalHealthbarSlider.maxValue = health;
        globalHealthbarSlider.value = health;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Invoke(nameof(PlayDialog), 0.1f);
    }

    [Server]
    void PlayDialog()
    {
        if (Random.Range(1, 4) == 1) RpcPlayDialogSound();
    }

    [ServerCallback]
    private void Update()
    {
        timer += Time.deltaTime;
        footstepTimer += Time.deltaTime;
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
                footstepTimer = footstepInterval;
                animator.SetBool("isRuning", false);
                navMeshAgent.isStopped = true;
                // look at enemy
                Vector3 dir = target.GetPosision() - creepModel.transform.position;
                dir.y = 0f;
                if (dir != Vector3.zero) creepModel.transform.rotation =
                    Quaternion.LookRotation(dir) * Quaternion.Euler(0, 0, 0);

                if (timer >= attackInterval)
                {
                    timer = 0;
                    RpcPlayMuzzleFlash();
                    dir = (target.GetPosision() - transform.position).normalized;

                    dir += Camera.main.transform.right * Random.Range(-spread, spread);
                    dir += Camera.main.transform.up * Random.Range(-spread, spread);

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

                        SpawnEffect(hit.point, Quaternion.LookRotation(hit.normal));
                    }
                }
            }
            else // go closer (target its not in attack range)
            {
                navMeshAgent.SetDestination(target.GetPosision());
                navMeshAgent.isStopped = false;
                animator.SetBool("isRuning", true);

                if (footstepTimer >= footstepInterval)
                {
                    footstepTimer = 0;
                    RpcPlayFootstep();
                }
            }
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

        fireAudioSource.pitch = Random.Range(0.9f, 1.1f);
        fireAudioSource.PlayOneShot(fireSound);

        Destroy(obj, 2f);
    }

    [ClientRpc]
    void RpcPlayDialogSound()
    {
        dialogAudioSource.PlayOneShot(dialogSound);
    }

    [ClientRpc]
    void RpcPlayFootstep()
    {
        if (footstepSounds.Length == 0 || walkAudioSource == null)
            return;
        walkAudioSource.PlayOneShot(

            footstepSounds[Random.Range(0, footstepSounds.Length)]);
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

    void OnHealthChanged(float oldValue, float newValue)
    {
        globalHealthbarSlider.value = newValue;
    }
}

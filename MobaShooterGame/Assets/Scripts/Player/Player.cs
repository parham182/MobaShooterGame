using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour, IDamageable
{
    public float maxHealth = 100;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public float currentHealth;

    [SyncVar(hook = nameof(OnSideChanged))]
    public string playerSide;

    public Gun gun;

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
    }

    public override void OnStartClient()
    {
        if (gun != null)
            gun.side = playerSide;
    }

    [Server]
    public void TakeDamage(float damage, damageableType attackerType, string attackerSide)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Respawn();
        }
    }

    [Server]
    public void Respawn()
    {
        // transform.position = playerSide == "red"
        //     ? SpawnManager.instance.redTeamSpawnPoint.position
        //     : SpawnManager.instance.blueTeamSpawnPoint.position;

        if (playerSide == "red") 
            GetComponent<NetworkTransformReliable>()
            .RpcTeleport(SpawnManager.instance.redTeamSpawnPoint.position);
        else
            GetComponent<NetworkTransformReliable>()
            .RpcTeleport(SpawnManager.instance.blueTeamSpawnPoint.position);

        currentHealth = maxHealth;
    }

    void OnHealthChanged(float oldValue, float newValue)
    {
        if (!isLocalPlayer) return;

        Healthbar.instance.slider.maxValue = maxHealth;
        Healthbar.instance.slider.value = newValue;
        Healthbar.instance.healthText.text = $"{newValue}/{maxHealth}";
    }

    void OnSideChanged(string oldValue, string newValue)
    {
        if (gun != null)
            gun.side = newValue;
    }

    public string DamageableSide() => playerSide;

    public damageableType GetDamageableType() => damageableType.Player;

    public Vector3 GetPosision() => transform.position;
}

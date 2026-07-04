using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour, IDamageable
{
    public float maxHealth = 100;
    public float baseArmor = 10;
    public float healthRegen = 1;

    public int level = 1;

    public float expNeedToLevelUp = 100;
    public float currentExp;

    [SyncVar]
    public float currentHealth;
    public int playerSide; // 0 = server | 1 = client
    public bool isInSop;
    public int coins;
    public Gun gun;

    private float timer;

    private void Start()
    {
        Respawn();
    }

    private void Update()
    {
        // health regen
        if (currentHealth < maxHealth) timer += Time.deltaTime;
        if (timer >= 1)
        {
            timer = 0;
            currentHealth += healthRegen;
            if (currentHealth > maxHealth) currentHealth = maxHealth;

            UpdateUI();
        }
    }

    public void GetExp(float exp)
    {
        currentExp += exp;

        while (currentExp >= expNeedToLevelUp)
        {
            currentExp -= expNeedToLevelUp;
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;

        maxHealth += 20;
        baseArmor += 2;
        healthRegen += 1;

        expNeedToLevelUp *= 1.1f;

        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        // calc damage reduction from armor
        float reduction =
            (0.06f * baseArmor) /
            (1 + 0.06f * Mathf.Abs(baseArmor));
        float multiplier = 1 - reduction;
        float finalDamage = damage * multiplier;
        currentHealth -= finalDamage;

        UpdateUI();
        
        if (currentHealth <= 0)
        {
            // TODO: respawn with a delay
            Respawn();
        }
    }

    private void UpdateUI()
    {
        if (!isLocalPlayer) return;

        Healthbar.instance.slider.value = currentHealth;
        Healthbar.instance.healthText.text = currentHealth + "/" + maxHealth;
        Healthbar.instance.healthRegenText.text = healthRegen + "/s";
    }

    private void Respawn()
    {
        if (isClient && !isServer)
        {
            transform.position = SpawnManager.instance.redTeamSpawnPoint.position;
            playerSide = 1;
        } else if (isServer)
        {
            transform.position = SpawnManager.instance.blueTeamSpawnPoint.position;
            playerSide = 0;
        }

        currentHealth = maxHealth;
        gun.side = playerSide;
    }

    public int DamageableSide()
    {
        return playerSide;
    }

    public damageableType GetDamageableType()
    {
        return damageableType.Player;
    }

    public Vector3 GetPosision()
    {
        return new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }
}

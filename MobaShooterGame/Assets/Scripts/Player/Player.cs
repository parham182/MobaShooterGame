using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    public float maxHealth = 100;
    public float baseArmor = 10;
    public float healthRegen = 1;

    public int level = 1;

    public float expNeedToLevelUp = 100;
    public float currentExp;

    public float currentHealth;

    [Header("UI")]
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text healthText;
    [SerializeField] TMP_Text healthRegenText;

    private float timer;

    private void Start()
    {
        currentHealth = maxHealth;
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
        if (!isServer) return;

        // calc damage reduction from armor
        float reduction =
            (0.06f * baseArmor) /
            (1 + 0.06f * Mathf.Abs(baseArmor));
        float multiplier = 1 - reduction;
        float finalDamage = damage * multiplier;
        currentHealth -= finalDamage;

        UpdateUI();
        
        if (currentHealth <= 0) print("Player died");
    }

    private void UpdateUI()
    {
        slider.value = currentHealth;
        healthText.text = currentHealth + "/" + maxHealth;
        healthRegenText.text = healthRegen + "/s";
    }
}

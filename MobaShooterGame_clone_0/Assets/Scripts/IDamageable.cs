using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(float damage);

    public string DamageableSide();

    public damageableType GetDamageableType();

    public Vector3 GetPosision();
}

public enum damageableType
{
    Player,
    Creep,
    Building
}
using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(float damage);

    public int DamageableSide();

    public damageableType GetDamageableType();

    public Vector3 GetPosision();
}

public enum damageableType
{
    Player,
    Creen,
    Building
}
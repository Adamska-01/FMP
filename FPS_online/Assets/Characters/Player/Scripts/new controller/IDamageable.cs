public interface IDamageable
{
    public void TakeDamage(float _dmg);
    public void Heal(float value);
    public bool CanHeal();
}

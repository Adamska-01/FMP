public interface IDamageable
{
    public void TakeDamage(float _dmg, string _damager = "");
    public void Heal(float value);
    public bool CanHeal();
}

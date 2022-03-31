using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyStats : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject deathEffect;

    //max values
    public float MAX_HEALTH_VALUE = 100.0f;
    public float MAX_ARMOUR_VALUE = 50.0f;

    //health
    private float HealthValue;
    //Stamina
    private float ArmourValue;

    private bool isDead;
     
    void Start()
    {
        HealthValue = MAX_HEALTH_VALUE;
        ArmourValue = MAX_ARMOUR_VALUE;

        AudioSource source = SoundManager.instance.PlaySoundAndReturn(SoundManagerConstants.Clips.DUMMY_SPAWN, SoundManagerConstants.AudioOutput.SFX, transform.position, 0.8f);
        source.spatialBlend = 0.0f;
    }

    //Changed by Mattie to FixedUpdate
    void FixedUpdate()
    {
        updateUI();
    }


    private void updateUI() //For now there is no UI
    {
        HealthValue = Mathf.Clamp(HealthValue, 0, MAX_HEALTH_VALUE);
        ArmourValue = Mathf.Clamp(ArmourValue, 0, MAX_ARMOUR_VALUE);
    }

    public bool CanHeal()
    {
        return false; 
    }

    public void Heal(float value)
    { }

    public void TakeDamage(float damage, string _damager, int _actor)
    {
        if (!isDead)
        { 
            if (ArmourValue > 0.0f)
            {
                ArmourValue -= damage;
                if (ArmourValue < 0.0f)
                    HealthValue += ArmourValue;
            }
            else
                HealthValue -= damage;

            if (HealthValue <= 0)
            {
                AudioSource source = SoundManager.instance.PlaySoundAndReturn(SoundManagerConstants.Clips.DUMMY_DEATH, SoundManagerConstants.AudioOutput.SFX, transform.position);  
                source.priority = 0;
                source.spatialBlend = 0.0f;

                Instantiate(deathEffect, transform.position, transform.rotation);
                
                Destroy(gameObject);
            }
        }
    } 
}

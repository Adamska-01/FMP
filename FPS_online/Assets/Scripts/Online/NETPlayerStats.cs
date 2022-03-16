using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NETPlayerStats : MonoBehaviour, IDamageable
{
    //max values
    public float MAX_HEALTH_VALUE = 100.0f;
    public float MAX_ARMOUR_VALUE = 50.0f;

    //health
    private float HealthValue;
    //Stamina
    private float ArmourValue;

    private bool isDead;

    private PhotonView pv;
    private PlayerManager playerManager;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();  

        playerManager = PhotonView.Find((int)pv.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    void Start()
    { 
        pv = GetComponent<PhotonView>();

        HealthValue = MAX_HEALTH_VALUE;
        ArmourValue = MAX_ARMOUR_VALUE;

        NETUIController.instance.healthText.text = HealthValue.ToString();
        NETUIController.instance.armourText.text = ArmourValue.ToString();
    }

    //Changed by Mattie to FixedUpdate
    void FixedUpdate()
    {
        updateUI();
    }


    private void updateUI() //For now there is no UI
    {
        if (!pv.IsMine)
            return;

        HealthValue = Mathf.Clamp(HealthValue, 0, MAX_HEALTH_VALUE);
        ArmourValue = Mathf.Clamp(ArmourValue, 0, MAX_ARMOUR_VALUE);

        NETUIController.instance.healthText.text = HealthValue.ToString();
        NETUIController.instance.armourText.text = ArmourValue.ToString();
    }

    public void TakeDamage(float damage, string _damager, int _actor)
    {
        pv.RPC("RPC_TakeDamage", RpcTarget.All, damage, _damager, _actor);
    }

    [PunRPC] private void RPC_TakeDamage(float damage, string _damager, int _actor)
    {
        //Make sure this runs only on the victim's client
        if (!pv.IsMine)
            return;

        if (!isDead)
        {
            Debug.Log("Took Damage: " + damage);
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
                isDead = true;

                //Sound
                AudioSource audioSource = SoundManager.instance.PlaySoundAndReturn(SoundManagerConstants.Clips.DEATH, SoundManagerConstants.AudioOutput.SFX, transform.position);
                audioSource.maxDistance = 4.0f;

                playerManager.Die(_damager); 

                GetComponent<Animator>().SetBool(GetComponent<NETAnimationController>().DeathHash, true);

                MatchManager.instance.UpdateStatsSend(_actor, 0, 1);
            }
        }
    }

    public void Heal(float value)
    {
        if (!isDead)
        {
            HealthValue += value;
            updateUI();
        }
    }

    public bool CanHeal()
    {
        return HealthValue < MAX_HEALTH_VALUE;
    }


    //--------------------------------------------------
    //Getter and Setter
    public void SetIsDead(bool value)
    {
        isDead = value;
    }

    public void SetSanity(float value)
    {
        HealthValue = value;
    }

    public void SetStamina(float value)
    {
        ArmourValue = value;
    }

    public float GetHealth()
    {
        return HealthValue;
    }

    public float GetArmour()
    {
        return ArmourValue;
    }

    public bool IsDead()
    {
        return isDead;
    }
}

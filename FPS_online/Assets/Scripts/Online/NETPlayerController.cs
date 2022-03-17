using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class NETPlayerController : MonoBehaviourPunCallbacks
{
    [HideInInspector] public CharacterController characterController;
    private NETPlayerStats stats;
    private NETInputManager inputManager;
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private Animator animator;
    [SerializeField] private NETAnimationController animController;
    private Vector3 movementDir;

    [SerializeField] private float offsetFloorY = 0.4f;
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float speedMultiplier = 1.8f;
    private float aimSensitivity = 0.4f;
    public float sensitivityMultiplier = 1.0f;
    private float ADSsensitivityMultiplier = 0.11f;
    [SerializeField] private float jumpForce = 200.0f; 
    private float ySpeed;
    public bool IsRunning { get { return (!inputManager.Crouch && !inputManager.Back && inputManager.Run); } }

    private float verticalLookRotation = 0.0f;
    [SerializeField] private Transform gunTarget;
    [SerializeField] private Transform cameraTarget;

    //States 
    [HideInInspector] public bool isFiring;
    [HideInInspector] public bool isFiringSingleShot;
    [HideInInspector] public bool isReloading;
    [HideInInspector] public bool isAiming;
    [HideInInspector] public bool canReload;

    [SerializeField] private UpperBodyIK ik;

    //Guns
    [SerializeField] NETItem[] items;
    int itemIndex;
    int previousItemIndex = -1;

    private PhotonView pv;
    public PhotonView PV { get { return pv; } }

    private void Awake()
    {
        stats = GetComponent<NETPlayerStats>();
        characterController = GetComponent<CharacterController>();
        pv = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();
        inputManager = FindObjectOfType<NETInputManager>();

        if (PlayerPrefs.HasKey("Settings->General->Sensitivity"))
            sensitivityMultiplier = PlayerPrefs.GetFloat("Settings->General->Sensitivity");

        isReloading = false; 
    }

    private void Start()
    {  
        if (pv.IsMine)
        { 
            EquipItem(0);
        }
        else
        {
            cameraHolder.GetComponentInChildren<Camera>().gameObject.SetActive(false);
            Destroy(characterController);
            //Destroy(GetComponent<NETAnimationController>()); 
        }
    }

    void Update()
    {
        if (!pv.IsMine) //Return if this is not the local user 
            return;

        if(!stats.IsDead())
        {
            UpdateMovementInput();
            UpdatePhysics();
            UpdateWeapon();
        }
    }
       
    private void UpdateMovementInput()
    {
        Vector3 forward = inputManager.Forward ? transform.forward : inputManager.Back ? -1 * transform.forward : Vector3.zero;
        Vector3 sideway = inputManager.Left ? -1 * transform.right : inputManager.Right ? transform.right : Vector3.zero;
        Vector3 combinedInput = (forward + sideway).normalized;
         
        movementDir = new Vector3(combinedInput.x, 0f, combinedInput.z);
        ySpeed += Physics.gravity.y * Time.deltaTime;

        Look();

        if (characterController.isGrounded)
        {
            ySpeed = -0.5f;
            if (inputManager.Jump && !inputManager.Crouch)
            {
                ySpeed = jumpForce;
                animator.SetTrigger(animController.JumpHash);
            }
        }
        movementDir.y = ySpeed;
    }

    private void UpdateWeapon()
    {
        isAiming = inputManager.IsAiming;
        if (!isReloading)
        {
            //Switch guns with numbers
            for (int i = 0; i < items.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    EquipItem(i); 
                    break;
                }
            }
            //Switch guns with scroll wheel
            if (inputManager.SwitchWeaponUp)
            {
                if (itemIndex >= (items.Length - 1))
                    EquipItem(0);
                else
                    EquipItem(itemIndex + 1);
            }
            if (inputManager.SwitchWeaponDown)
            {
                if (itemIndex <= 0)
                    EquipItem(items.Length - 1);
                else
                    EquipItem(itemIndex - 1);
            }
        }

        //Fire
        FireWeapon();
        TryToReloadWeapon();

        //Update UI
        NETGun gun = ((NETGun)items[itemIndex]);
        string ammoInMagazine = gun.currentAmmoInMagazine.ToString();
        string ammoAvailable = gun.ammoAvailable > 999 ? "\u221E" : gun.ammoAvailable.ToString();
        if(itemIndex == 2)
            NETUIController.instance.ammunitionText.text = string.Empty;
        else
            NETUIController.instance.ammunitionText.text = ammoInMagazine + "/" + ammoAvailable;
    }


    private void EquipItem(int _index)
    {
        if (_index == previousItemIndex) //safe check 
            return;

        //Set current index
        itemIndex = _index;

        //Set current gun to true
        foreach (var item in items[itemIndex].itemObject)
        {
            item.SetActive(true); 
        }

        //Set the previous gun to false
        if (previousItemIndex != -1)
        {
            foreach (var item in items[previousItemIndex].itemObject)
            {
                item.SetActive(false);
            } 
        }
        previousItemIndex = itemIndex;

        ik.StartCoroutine(ik.ChangeLeftArmTarget(((GunInfo)items[itemIndex].itemInfo).leftHandTarget));
        
        //Change left arm target
        if (pv.IsMine)
        { 
            //Update UI
            NETUIController.instance.SelectWeapon(itemIndex);

            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    //----------------------Photon Callbacks----------------------
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {//Called every time a custom property updates 
        if (pv && !pv.IsMine && targetPlayer == pv.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    } 

    private void FireWeapon()
    { 
        if (inputManager.FireSingleShot && !isReloading && (items[itemIndex].TryGetComponent<NETSingleShotGun>(out var ssg) || items[itemIndex].TryGetComponent<NETMeleeWeapon>(out var melee)))
        {
            if (items[itemIndex].Use())  
                animator.SetTrigger(animController.FireHash);
        }
        else if (inputManager.AutomaticShot && !isReloading && items[itemIndex].TryGetComponent<NETAutomaticGun>(out var ag))
        {
        
            if(items[itemIndex].Use())
                animator.SetTrigger(animController.FireHash);
        }
        else
        {
            isFiring = false;
            isFiringSingleShot = false;
        }
    }

    private void TryToReloadWeapon()
    {
        if (!isReloading && inputManager.Reload && items[itemIndex].CanReload())
        {
            isReloading = true;
            animator.SetTrigger(animController.ReloadHash);

            if (items[itemIndex].TryGetComponent<NETSingleShotGun>(out var ssg))
                SoundManager.instance.PlaySound(SoundManagerConstants.Clips.RELOAD_HANDGUN, SoundManagerConstants.AudioOutput.SFX, gameObject);
            else 
                SoundManager.instance.PlaySoundAndReturn(SoundManagerConstants.Clips.RELOAD_RIFLE, SoundManagerConstants.AudioOutput.SFX, gameObject);  
        }
    }

    public void ReloadWeapon()
    {
        isReloading = false;
        items[itemIndex].Reload();
    }

    private void UpdatePhysics()
    {
        //Update velocity
        characterController.Move((movementDir * (IsRunning ? movementSpeed * speedMultiplier : movementSpeed) * Time.deltaTime));
    }

    private void Look()
    {
        float sensitivity = isAiming ? ADSsensitivityMultiplier : (aimSensitivity * sensitivityMultiplier);

        //Rotate player 
        transform.Rotate(Vector3.up * inputManager.XLookAxis * sensitivity);

        //Rotate camera
        verticalLookRotation += inputManager.YLookAxis * sensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -70.0f, 70f); 
        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation; 
    }  
}

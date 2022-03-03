using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class NETPlayerController : MonoBehaviourPunCallbacks
{
    private NETInputManager inputManager;
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private Animator animator;
    [SerializeField] private NETAnimationController animController;
    private Vector3 movementDir;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider col;
    [SerializeField] private float offsetFloorY = 0.4f;
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float speedMultiplier = 1.6f;
    [SerializeField] private float xAxisSensitivity = 0.2f;
    [SerializeField] private float yAxisSensitivity = 0.2f;
    private bool isGrounded;
    public bool IsGrounded { get { return isGrounded; } }
    [SerializeField] private float jumpForce = 200.0f;
    public float fallMultiplier;
    private Vector3 gravity;
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

    public GameObject groundCheck;

    private PhotonView pv; 

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();
        inputManager = FindObjectOfType<NETInputManager>(); 
        
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
            Destroy(rb);
            Destroy(GetComponent<NETAnimationController>());
            Destroy(groundCheck);
        }
    }

    void Update()
    {
        if (!pv.IsMine) //Return if this is not the local user 
            return;

        UpdateMovementInput();
        UpdateWeapon(); 
    }
     
    void FixedUpdate()
    {
        if (!pv.IsMine) //Return if this is not the local user 
            return;

        UpdatePhysics();        
    }
     

    private void UpdateMovementInput()
    {
        Vector3 forward = inputManager.Forward ? transform.forward : inputManager.Back ? -1 * transform.forward : Vector3.zero;
        Vector3 sideway = inputManager.Left ? -1 * transform.right : inputManager.Right ? transform.right : Vector3.zero;
        Vector3 combinedInput = (forward + sideway).normalized;
         
        movementDir = new Vector3(combinedInput.x, 0f, combinedInput.z);

        Look();
        if (inputManager.Jump && !inputManager.Crouch && isGrounded)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
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
            HUDController.instance.ammunitionText.text = string.Empty;
        else
            HUDController.instance.ammunitionText.text = ammoInMagazine + "/" + ammoAvailable;
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
            HUDController.instance.SelectWeapon(itemIndex);

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
        Debug.Log(inputManager.AutomaticShot);
        if (inputManager.FireSingleShot && (items[itemIndex].TryGetComponent<NETSingleShotGun>(out var ssg) || items[itemIndex].TryGetComponent<NETMeleeWeapon>(out var melee)))
        {
            if (items[itemIndex].Use())  
                animator.SetTrigger(animController.FireHash);
        }
        else if (inputManager.AutomaticShot && items[itemIndex].TryGetComponent<NETAutomaticGun>(out var ag))
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
        }
    }

    public void ReloadWeapon()
    {
        isReloading = false;
        items[itemIndex].Reload();
    }

    private void UpdatePhysics()
    {  
        if(rb.velocity.y < 0.0f) 
            gravity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime; 
        else 
            gravity = Vector3.zero; 

        //Update velocity
        rb.velocity = (movementDir * (IsRunning ? movementSpeed * speedMultiplier : movementSpeed )) + gravity; 
    }

    private void Look()
    {
        //Rotate player 
        transform.Rotate(Vector3.up * inputManager.XLookAxis);

        //Rotate camera
        verticalLookRotation += inputManager.YLookAxis * yAxisSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -70.0f, 70f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation; 
    } 

    public void SetGrounded(bool _grnd)
    {
        isGrounded = _grnd;
    }  
}
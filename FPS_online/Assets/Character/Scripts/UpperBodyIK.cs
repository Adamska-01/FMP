using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class UpperBodyIK : MonoBehaviour
{ 
    [Header("Final IK Modules")] 
    [SerializeField] private ArmIK leftArmIK = default;
    [SerializeField] private ArmIK rightArmIK = default;
    [SerializeField] private FullBodyBipedIK fbbIK = default; 

    [Header("Input")] 
    [SerializeField] private InputManager inputManager;

    private bool isIKActive = true;


    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
         
        rightArmIK.enabled = false;
        leftArmIK.enabled = false;
        fbbIK.enabled = false; 
    }

    void Update()
    {
        fbbIK.solver.FixTransforms();
        if(isIKActive)
        {
            rightArmIK.solver.FixTransforms();
            leftArmIK.solver.FixTransforms(); 
        }
    }

    void LateUpdate()
    {
        FBBIKUpdate();
        if(isIKActive)
        {
            ArmsIKUpdate();
        }
    } 


    private void ArmsIKUpdate()
    { 
        rightArmIK.solver.Update();
        leftArmIK.solver.Update();
    }

    private void FBBIKUpdate()
    {
        fbbIK.solver.Update(); 
    }

    public void ActivateIK() => isIKActive = true;
    public void DeactivateIK() => isIKActive = false;
}

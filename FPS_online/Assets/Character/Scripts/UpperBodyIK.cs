using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class UpperBodyIK : MonoBehaviour
{ 
    [Header("Final IK Modules")] 
    [SerializeField] private ArmIK leftArmIK;
    [SerializeField] private ArmIK rightArmIK;
    [SerializeField] private FullBodyBipedIK fbbIK;
    [SerializeField] private Transform leftHandTarget;

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

    public IEnumerator ChangeLeftArmTarget(Transform _t)
    {
        leftHandTarget.localPosition = new Vector3(_t.position.x, _t.position.y, _t.position.z);
        leftHandTarget.localRotation = new Quaternion(_t.rotation.x, _t.rotation.y, _t.rotation.z, _t.rotation.w);
        yield return null;
        //float elapsedTime = 0;
        //float waitTime = 0.7f;
        //Vector3 currentPos = leftHandTarget.position;
        //Quaternion currentRot = leftHandTarget.rotation;

        //while (elapsedTime < waitTime)
        //{
        //    leftHandTarget.localPosition = Vector3.Lerp(currentPos, new Vector3(_t.position.x, _t.position.y, _t.position.z), (elapsedTime / waitTime));
        //    leftHandTarget.localRotation = Quaternion.Lerp(currentRot, new Quaternion(_t.rotation.x, _t.rotation.y, _t.rotation.z, _t.rotation.w), (elapsedTime / waitTime));

        //    elapsedTime += Time.deltaTime;
        //    yield return null;
        //}
    }

    public void ActivateIK() => isIKActive = true;
    public void DeactivateIK() => isIKActive = false;
}

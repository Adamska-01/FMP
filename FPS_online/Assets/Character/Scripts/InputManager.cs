using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{ 
    [Header("Movement Input")] 
    protected bool forward;
    public bool Forward { get { return forward; } }
    protected bool left; 
    public bool Left { get { return left; } }
    protected bool right;
    public bool Right { get { return right; } }
    protected bool back;
    public bool Back { get { return back; } }
    protected bool run;
    public bool Run { get { return run; } }
    protected bool crouch;
    public bool Crouch { get { return crouch; } }

    [Header("Weapon Keys")]
    private KeyCode aimKey = KeyCode.Mouse1;
    protected bool isAiming;
    public bool IsAiming { get { return isAiming; } }

    [Header("Camera Axis")]
    private string verticalLookAxis = "Mouse Y";
    private string horizontalLookAxis = "Mouse X";
    private float xAxisSensitivity = 0.2f;
    private float yAxisSensitivity = 0.2f;
    protected float xAxis;
    public float XLookAxis { get { return xAxis; } }
    protected float yAxis;
    public float YLookAxis { get { return yAxis; } }


    private void Update()
    {
        HandleInput();
    }
    

    protected void HandleInput()
    {
        //Movement
        forward = Input.GetKey(KeyCode.W);
        left = Input.GetKey(KeyCode.A);
        back = Input.GetKey(KeyCode.S);
        right = Input.GetKey(KeyCode.D);
        run = Input.GetKey(KeyCode.LeftShift);
        crouch = Input.GetKeyDown(KeyCode.C) ? !crouch : crouch;

        //ADS
        isAiming = Input.GetKey(aimKey);

        //Camera rotate
        xAxis = Input.GetAxis(horizontalLookAxis) * xAxisSensitivity;
        yAxis = Input.GetAxis(verticalLookAxis) * yAxisSensitivity;
    }
}

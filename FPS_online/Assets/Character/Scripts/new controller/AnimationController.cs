using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private float velocityZ = 0.0f;
    private float velocityX = 0.0f;
    public float acceleration = 0.1f;
    public float deceleration = 0.5f;
    public float maximumWalkVelocity = 0.5f;
    public float maximumRunVelocity = 2.0f;
    private int XvelocityHash;
    private int ZvelocityHash;
    private int crouchHash;

    [Header("Input")]
    [SerializeField] private InputManager inputManager;

    void Start()
    {
        animator = GetComponent<Animator>();

        XvelocityHash = Animator.StringToHash("VelocityX");
        ZvelocityHash = Animator.StringToHash("VelocityZ");
        crouchHash = Animator.StringToHash("IsCrouched");
    }


    void Update()
    {
        //Get key input from player
        bool forwardPressed = inputManager.Forward;
        bool leftPressed = inputManager.Left;
        bool rightPressed = inputManager.Right;
        bool backPressed = inputManager.Back;
        bool runPressed = inputManager.Run;

        //set current maxVelocity (running)
        float currentMaxVelocity = (runPressed && !backPressed) ? maximumRunVelocity : maximumWalkVelocity;

        ChangeVelocity(forwardPressed, leftPressed, rightPressed, backPressed, runPressed, currentMaxVelocity);
        LockOrResetVelocity(forwardPressed, leftPressed, rightPressed, backPressed, runPressed, currentMaxVelocity);

        //Update animation
        animator.SetFloat(XvelocityHash, velocityX);
        animator.SetFloat(ZvelocityHash, velocityZ);
        animator.SetBool(crouchHash, inputManager.Crouch);
    }

    //Check for input and assign velocities 
    private void ChangeVelocity(bool _forwardPressed, bool _leftPressed, bool _rightPressed, bool _backPressed, bool _runPressed, float _currentMaxVelocity)
    {
        if (_forwardPressed && velocityZ < _currentMaxVelocity) //forward
        {
            velocityZ += Time.deltaTime * acceleration;
        }
        if (_leftPressed && velocityX > -_currentMaxVelocity) //left
        {
            velocityX -= Time.deltaTime * acceleration;
        }
        if (_rightPressed && velocityX < _currentMaxVelocity) //right
        {
            velocityX += Time.deltaTime * acceleration;
        }
        if (_backPressed && velocityZ > -_currentMaxVelocity) //Back
        {
            velocityZ -= Time.deltaTime * acceleration;
        }

        //decrese velocityZ
        if (!_forwardPressed && velocityZ > .0f)
        {
            velocityZ -= Time.deltaTime * deceleration;
        }
        //increase velocityX if left is not pressed and velocityX < 0
        if (!_leftPressed && velocityX < .0f)
        {
            velocityX += Time.deltaTime * deceleration;
        }
        //decrease velocityX if right is not pressed and velocityX > 0
        if (!_rightPressed && velocityX > .0f)
        {
            velocityX -= Time.deltaTime * deceleration;
        }
        //Decrease velocityZ if back is not pressed and velocityZ < 0
        if (!_backPressed && velocityZ < .0f)
        {
            velocityZ += Time.deltaTime * deceleration;
        }
    }

    private void LockOrResetVelocity(bool _forwardPressed, bool _leftPressed, bool _rightPressed, bool _backPressed, bool _runPressed, float currentMaxVelocity)
    {
        //reset velocityZ (safe check)
        if (!_forwardPressed && !_backPressed && velocityZ != .0f && (velocityZ > -.1f && velocityZ < .1f))
        {
            velocityZ = .0f;
        }
        //reset velocityX (safe check)
        if (!_leftPressed && !_rightPressed && velocityX != .0f && (velocityX > -.1f && velocityX < .1f))
        {
            velocityX = .0f;
        }

        //lock forward
        if (_forwardPressed && _runPressed && velocityZ > currentMaxVelocity)
        {
            velocityZ = currentMaxVelocity;
        }
        //decelerate to the maximum walk veloxity
        else if (_forwardPressed && velocityZ > currentMaxVelocity)
        {
            velocityZ -= Time.deltaTime * deceleration;
            //round to the currentMaxVel if within offset
            if (velocityZ > currentMaxVelocity && velocityZ < (currentMaxVelocity + .05f))
            {
                velocityZ = currentMaxVelocity;
            }
        }
        else if (_forwardPressed && velocityZ < currentMaxVelocity && velocityZ > currentMaxVelocity - .05f)
        {
            velocityZ = currentMaxVelocity;
        }

        //lock left
        if (_leftPressed && _runPressed && velocityX < -currentMaxVelocity)
        {
            velocityX = -currentMaxVelocity;
        }
        //decelerate to the maximum walk veloxity
        else if (_leftPressed && velocityX < -currentMaxVelocity)
        {
            velocityX += Time.deltaTime * deceleration;
            //round to the currentMaxVel if within offset
            if (velocityX < -currentMaxVelocity && velocityX > (-currentMaxVelocity - .05f))
            {
                velocityX = -currentMaxVelocity;
            }
        }
        else if (_leftPressed && velocityX > -currentMaxVelocity && velocityX < (-currentMaxVelocity + .05f))
        {
            velocityX = -currentMaxVelocity;
        }

        //lock right
        if (_rightPressed && _runPressed && velocityX > currentMaxVelocity)
        {
            velocityX = currentMaxVelocity;
        }
        //decelerate to the maximum walk veloxity
        else if (_rightPressed && velocityX > currentMaxVelocity)
        {
            velocityX -= Time.deltaTime * deceleration;
            //round to the currentMaxVel if within offset
            if (velocityX > currentMaxVelocity && velocityX < (currentMaxVelocity + .05f))
            {
                velocityX = currentMaxVelocity;
            }
        }
        else if (_rightPressed && velocityX < currentMaxVelocity && velocityX > (currentMaxVelocity - .05f))
        {
            velocityX = currentMaxVelocity;
        }

        //lock back
        if (_backPressed && velocityZ < -currentMaxVelocity)
        {
            velocityZ += Time.deltaTime * deceleration;
            //round to the currentMaxVel if within offset
            if (velocityZ < -currentMaxVelocity && velocityZ > (-currentMaxVelocity - .05f))
            {
                velocityZ = -currentMaxVelocity;
            }
        }
        else if (_backPressed && velocityZ > -currentMaxVelocity && velocityZ < (-currentMaxVelocity + .05f))
        {
            velocityZ = -currentMaxVelocity;
        }
    }
}

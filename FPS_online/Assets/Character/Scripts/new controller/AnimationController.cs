using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private float velocity;
    public float acceleration = 0.1f;
    public float deceleration = 0.5f;
    int velocityHash;


    void Start()
    {
        animator = GetComponent<Animator>();

        velocityHash = Animator.StringToHash("Velocity");
    }


    void Update()
    {
        //Get key input from player
        bool forwardPressed = Input.GetKey(KeyCode.W);
        bool runPressed = Input.GetKey(KeyCode.LeftShift);

        if(forwardPressed && velocity < 1.0f)
        {
            velocity += Time.deltaTime * acceleration;
        }
        if (!forwardPressed && velocity > 0.0f)
        {
            velocity -= Time.deltaTime * deceleration;
        }
        if (!forwardPressed && velocity < 0.0f)
        {
            velocity = 0.0f;
        }

        animator.SetFloat(velocityHash, velocity);
    }
}

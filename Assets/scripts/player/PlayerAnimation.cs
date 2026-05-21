using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimation : MonoBehaviour
{
    private PlayerMovement pm;
    private Animator anim;
    private PlayerInput input;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();
        pm = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if(input.actions["walk"].ReadValue<float>() < 0){
            transform.localScale = new Vector2(-1, transform.localScale.y);
        } else{
            transform.localScale = new Vector2(1, transform.localScale.y);
        }
        switch(pm.state){
            case PlayerMovement.playerState.idle:
                anim.speed = 1;
                anim.SetInteger("state", 0);
                break;
                
            case PlayerMovement.playerState.walk:
                anim.speed = 0.75f;
                anim.SetInteger("state", 1);
                break;

            case PlayerMovement.playerState.run:
                anim.speed = 1;
                anim.SetInteger("state", 1);
                break;

            case PlayerMovement.playerState.maxspeed:
                anim.speed = 1.125f;
                anim.SetInteger("state", 1);
                break;

            case PlayerMovement.playerState.jump:
                anim.speed = 1;
                anim.SetInteger("state", 2);
                break;

            case PlayerMovement.playerState.fall:
                anim.speed = 1;
                anim.SetInteger("state", 3);
                break;
        }
    }
}

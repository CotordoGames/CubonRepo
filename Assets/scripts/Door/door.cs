using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class door : MonoBehaviour
{
    private checkfordoornum player;
    public int doorID;
    public static int currentID;
    [SerializeField] Animator anim;
    public string nextSceneName;
    bool isTouchingdoor = false;
    private PlayerInput input;

    private void Awake()
    {
        player = GameObject.FindAnyObjectByType<checkfordoornum>();
        player.doors.Add(this);
        input = GetComponent<PlayerInput>();
    }
    private void OnTriggerEnter2D(Collider2D Collision)
    {
        isTouchingdoor = false;
        if (Collision.gameObject.CompareTag("Player"))  // Check if the colliding object is the player
        {
            isTouchingdoor = true;
        }
        else if(!Collision.gameObject.CompareTag("Player"))
        {
            isTouchingdoor = false;
        }
    }
    private void OnTriggerExit2D(Collider2D collision){
        if(collision.gameObject.CompareTag("Player")){
            isTouchingdoor = false;
        }
    }
    private void Update()
    {
        if (isTouchingdoor && input.actions["interact"].IsPressed())
        {
            StartCoroutine(SceneBegin());
        }
    }

    private IEnumerator SceneBegin()
    {
        currentID = doorID;
        if(anim != null)
        {
            anim.SetTrigger("start");
        }
        yield return new WaitForSeconds(.5f);
        SceneManager.LoadScene(nextSceneName);
        if(anim != null)
        {
            anim.SetTrigger("end");
        }
        
    }
}

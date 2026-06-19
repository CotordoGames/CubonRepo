using UnityEngine;
using UnityEngine.InputSystem;

public class BasicNPC : MonoBehaviour
{
    private DialogueManager dm;
    public Dialogue dialogue;
    
    public bool onlyTalkOnInteract = true;
    
    [SerializeField] private bool touching = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            touching = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            touching = false;
        }
    }

    void Start()
    {
        dm = FindAnyObjectByType<DialogueManager>();
    }
    
    // Update is called once per frame
    void Update()
    {
        if(touching && !dm.isTalking && !onlyTalkOnInteract)
        {
            TriggerDialogue();
        }
        else if(touching && onlyTalkOnInteract && !dm.isTalking)
        {
            if(dm.input.actions["interact"].IsPressed())
            {
                TriggerDialogue();
                Debug.Log("interact");
            }
        }
    }
    
    public void TriggerDialogue()
    {
        dm.StartDialogue(dialogue);
    }
}

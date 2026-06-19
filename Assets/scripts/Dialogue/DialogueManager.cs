using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using FMODUnity;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public PlayerInput input;
    public EventReference soundEffect;
    public EventReference textSound;
    
    private string sentence;

    public PlayerMovement pm;
    public TextMeshProUGUI dialogueText;
    public Image dialoguePortrait;
    public bool isTalking = false;
    public bool isTyping = false;
    
    public Animator animator;
    private Dialogue currentDialogue;
    
    public Queue<DialogueLine> dialogueLines;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isTalking = false;
        dialogueLines = new Queue<DialogueLine>();
        input = GetComponent<PlayerInput>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        isTalking = true;
        animator.SetBool("open", true);
        currentDialogue = dialogue;

        dialogueLines.Clear();
        pm.rb.linearVelocityX = 0;
        pm.enabled = false;
        foreach (var line in dialogue.lines)
        {
            dialogueLines.Enqueue(line);
        }

        DisplayNewSentence();
    }

    public void DisplayNewSentence()
    {
        if (dialogueLines.Count == 0)
        {
            EndDialogue();
            pm.enabled = true;
            return;
        }

        DialogueLine line = dialogueLines.Dequeue();
        dialoguePortrait.sprite = line.portrait;
        dialogueText.color = line.color;
        soundEffect = line.sound;
        RuntimeManager.PlayOneShot(soundEffect,  transform.position);
        
        sentence = line.sentence;
        float speed = line.speed;
        StopAllCoroutines();
        StartCoroutine(TypeSentence(speed));
    }

    private int idx;
    IEnumerator TypeSentence(float speed)
    {
        isTyping = true;
        idx = 0;
        dialogueText.text = "";
        foreach (char letter in sentence)
        {
            idx++;

            if (!sentence.ToCharArray()[idx - 1].Equals(' '))
            {
                RuntimeManager.PlayOneShot(textSound,  transform.position);
            }

            if (sentence.ToCharArray()[idx - 1].Equals(',') || sentence.ToCharArray()[idx - 1].Equals('.') ||
                sentence.ToCharArray()[idx - 1].Equals('!'))
            {
                yield return new WaitForSeconds(speed * 1.25f);
            }
            else
            {
                yield return new WaitForSeconds(speed);
            }
            dialogueText.text = sentence.Substring(0, idx);
        }
        isTyping = false;
    }

    public void EndDialogue()
    {
        isTalking = false;
        animator.SetBool("open", false);
        pm.enabled = true;
    }
    
    public void Update()
    {
        foreach(var action in input.actions)
        {
            Debug.Log(action.name);
        }
        if(isTyping && input.actions["skip"].WasPressedThisFrame())
        {
            StopAllCoroutines();
            idx = sentence.Length;
            dialogueText.text = sentence;
            isTyping = false;
            Debug.Log(sentence);
        }
        if(!isTyping && input.actions["advance"].WasPressedThisFrame())
        {
            DisplayNewSentence();
            Debug.Log(sentence);
        }
    }
}

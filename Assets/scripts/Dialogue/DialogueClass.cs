using UnityEngine;
using FMODUnity;

[System.Serializable]
public class Dialogue
{
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    public Sprite portrait;
    public float speed;
    public Color color;
    public EventReference sound;
    public string animation;
    
    [TextArea(3, 12)]
    public string sentence;

    // both variables are supposed to be under this so rider stop FUCKING WITH IT
    [Header("Camera")] 
    public Transform cameraPosition;
    public float cameraSpeed;
}

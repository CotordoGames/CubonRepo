using UnityEngine;

public class ParaLaxxLayer : MonoBehaviour
{
    public Transform cam;
    public float speed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(cam.position.x * speed, cam.position.y * speed, transform.position.z);
    }
}

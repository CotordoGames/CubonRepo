using UnityEngine;

public class ParaLaxxLayer : MonoBehaviour
{
    public Transform camera;
    public float speed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(camera.position.x * speed, camera.position.y * speed, transform.position.z);
    }
}

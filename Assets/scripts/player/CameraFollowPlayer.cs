using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Threading;
using System.Collections;

public class CameraFollowPlayer : MonoBehaviour
{

    private Coroutine shakeRoutine;
    private Vector3 shakeOffset;
    private PlayerInput input;
    public Transform target;
    public float speed;

    public float lookAhead;

    public float lookAheadTime;

    void Start()
    {
        input = GetComponent<PlayerInput>();
    }

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(Mathf.Lerp(target.position.x, target.position.x + lookAhead * input.actions["left-right"].ReadValue<float>(), lookAheadTime * Time.deltaTime * 60), target.position.y, -10f) + shakeOffset, speed * Time.deltaTime * 60);
    }

    public void ShakeCamera(float intensity, float length)
    {
        if(shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }
        shakeRoutine = StartCoroutine(ShakeRoutine(intensity, length));
    }

    private IEnumerator ShakeRoutine(float intensity, float length)
    {
        float elapsed = 0f;

        while(elapsed < length)
        {
            shakeOffset = new Vector3(
                UnityEngine.Random.Range(-1f, 1f) * intensity,
                UnityEngine.Random.Range(-1f, 1f) * intensity,
                0f
            );

            elapsed += Time.deltaTime;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }
}
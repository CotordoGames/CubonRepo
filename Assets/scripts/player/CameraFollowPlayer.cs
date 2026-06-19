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

    private float currentLookAhead;

    void Start()
    {
        input = GetComponent<PlayerInput>();
    }

    void LateUpdate()
    {
        float TargetLookAhead = lookAhead * input.actions["left-right"].ReadValue<float>();
        currentLookAhead = Mathf.Lerp(currentLookAhead, TargetLookAhead, lookAheadTime * Time.deltaTime * 60);

        float TargetX = target.position.x + currentLookAhead;
        float TargetY = target.position.y;

        transform.position = new Vector3(
            TargetX,
            Mathf.Lerp(transform.position.y, TargetY, speed * Time.deltaTime * 60),
            -10f
            ) + shakeOffset;
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
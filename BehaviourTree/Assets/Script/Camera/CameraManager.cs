using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager _Instance;
    public static CameraManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType(typeof(CameraManager)) as CameraManager;
            }

            return _Instance;
        }
    }

    public float shakeDuration;
    public float shakemagnitude;
    public Vector3 shakeStartPos;

    public Vector3 StartPos;
    public Transform CameraTarget;
    public float distance;
    public float cameraSpeed;
    private bool battleMode;
    public bool setBattleMode
    {
        set { battleMode = value; }
    }

    public void CameraShacking(float duration = 0.15f, float magnitude = 0.8f)
    {
        shakeStartPos = transform.position;
        shakeDuration = duration;
        shakemagnitude = magnitude;

        StartCoroutine(Shaking());
    }

    IEnumerator Shaking()
    {
        float timer = 0;

        while (timer <= shakeDuration)
        {
            transform.localPosition = (Vector3)Random.insideUnitSphere * shakemagnitude + shakeStartPos;

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = shakeStartPos;
        yield break;
    }

    public void CameraBattleMode(bool on)
    {
        battleMode = on;
    }

    private void LateUpdate()
    {
        if(battleMode)
        {
            Vector3 wantPosition = CameraTarget.position + -distance * transform.forward;
            transform.position = Vector3.Lerp(transform.position, wantPosition, Time.deltaTime * cameraSpeed);
        }
        else if(shakeStartPos != transform.position)
        {
            transform.position = Vector3.Lerp(transform.position, StartPos, Time.deltaTime * cameraSpeed * 2);
        }
    }
}

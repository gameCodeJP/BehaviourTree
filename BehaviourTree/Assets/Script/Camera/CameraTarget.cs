using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    Transform Target = null;

    private void Update()
    {
        if(Target != null)
        {
            transform.position = Target.position;
        }
    }

    public void SetPosition(Vector3 pos)
    {
        Target = null;
        transform.position = pos;
    }

    public void SetTarget(Transform transform)
    {
        Target = transform;
    }
}

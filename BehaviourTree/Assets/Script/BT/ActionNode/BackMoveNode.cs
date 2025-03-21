using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackMoveNode : ActionNode
{
    public override void Init()
    {
    }

    protected override void OnStart()
    {
        OwnerTransform.LookAt(Info.startPos);
        CameraManager.Instance.setBattleMode = false;
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if ((OwnerTransform.position - Info.startPos).magnitude > 0.1f)
        {
            OwnerTransform.position = Vector3.MoveTowards(OwnerTransform.position, Info.startPos, Time.deltaTime * 10f);
            return State.Running;
        }

        OwnerTransform.localRotation = Info.startRotation;
        return State.Success;
    }
}

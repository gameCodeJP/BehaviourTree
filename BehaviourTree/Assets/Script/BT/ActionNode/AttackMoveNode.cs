using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackMoveNode : ActionNode
{
    public override void Init()
    {
    }

    protected override void OnStart()
    {
        OwnerTransform.LookAt(OwnerBattle.curTargets[0].transform.position);
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        //타켓을 쫓아감
        if (Info.heroseDate.AttackType == AttackType.CLOSE && (OwnerTransform.position - OwnerBattle.curTargets[0].transform.position).magnitude > 3f)
        {
            OwnerTransform.position = Vector3.MoveTowards(OwnerTransform.position, OwnerBattle.curTargets[0].transform.position, Time.deltaTime * 10f);
            return State.Running;
        }

        //MP업
        Info.runTimeStat.CurMP += Random.Range(0, 50);
        return State.Success;
    }
}

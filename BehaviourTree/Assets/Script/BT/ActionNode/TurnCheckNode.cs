using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnCheckNode : ActionNode
{
    public override void Init()
    {
    }

    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if(!Info.victory && BattleManager.Instance.TurnChracter == Info.ID)
        {
            //자신의 턴일 시
            OwnerBattle.turnIndigate.SetActive(true);
            return State.Success;
        }

        return State.Failure;
    }
}

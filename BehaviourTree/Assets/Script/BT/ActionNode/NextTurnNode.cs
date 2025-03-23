using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextTurnNode : ActionNode
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
        BattleManager.Instance.NextChracter();
        Info.UseSkill = false;
        Info.curSkillIndex = 0;
        OwnerBattle.turnIndigate.SetActive(false);
        return State.Success;
    }
}

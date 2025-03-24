using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillNode : ActionNode
{
    Vector3 SkillPos;

    public override void Init()
    {
    }

    //enum SkillState { Success, Running, Failure }

    protected override void OnStart()
    {
        if (Info.UseSkill)
        {
            Info.curSkillIndex = 0;
            SetSkillPosition();
        }
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (Info.UseSkill == false)
            return State.Failure;

        //스킬사용 위치까지 이동
        if (Info.skillDatas[0].SkillPosition == SkillPosition.Front
            && (OwnerTransform.position - SkillPos).magnitude > 0.5f)
        {
            OwnerTransform.position = Vector3.MoveTowards(OwnerTransform.position, SkillPos, Time.deltaTime * 10f);
            return State.Running;
        }

        //타켓이 한명이면
        if (OwnerBattle.curTargets.Count == 1) OwnerTransform.LookAt(OwnerBattle.curTargets[0].transform.position); 
        else OwnerTransform.rotation = Info.startRotation;

        return State.Success;
    }

    void SetSkillPosition()
    {
        SkillData curSkillData = Info.skillDatas[Info.curSkillIndex];

        if (curSkillData.SkillStartPosition == SkillStartPosition.None)
        {
            switch (curSkillData.SkillPosition)
            {
                case SkillPosition.Front:
                    GetFrontSkillPosition();
                    break;
                case SkillPosition.StartPos:
                    SkillPos = Info.transform.position;
                    if (curSkillData.SkillType != SKILLTYPE.ATTACK)
                    {
                        OwnerBattle.cameraTarget.SetTarget(OwnerTransform);
                    }
                    else
                    {
                        Vector3 TargetPoint = Info.playerType == PlayerType.Player ? Define.EnemyMidPosition : Define.TeamMidPosition;
                        OwnerBattle.cameraTarget.SetPosition(Vector3.Lerp(OwnerTransform.position ,TargetPoint, 0.6f));
                    }
                    break;
            }
        }
        else if(curSkillData.SkillStartPosition == SkillStartPosition.Mid)
        {
            SkillPos = Define.ActionFrontPoint;
            OwnerBattle.cameraTarget.SetTarget(OwnerTransform);
        }
        else
        {
            SkillPos = Info.playerType == PlayerType.Player ? Define.EnemyMidPosition : Define.TeamMidPosition;
            OwnerBattle.cameraTarget.SetTarget(OwnerTransform);
        }

        CameraManager.Instance.setBattleMode = true;
    }

    //근접공격 유형
    void GetFrontSkillPosition()
    {
        SkillData skillData = Info.skillDatas[Info.curSkillIndex];

        switch (skillData.TargetRange)
        {
            case TargetArea.FrontRow:
                SkillPos = Define.ActionFrontPoint;
                break;
            case TargetArea.Single:
                SkillPos = OwnerBattle.curTargets[0].transform.position +
                   (OwnerTransform.position - OwnerBattle.curTargets[0].transform.position).normalized * 2f;
                break;
            default:
                SkillPos = Info.playerType == PlayerType.Player ? Define.EnemyMidPosition : Define.TeamMidPosition;
                break;
        }

        OwnerBattle.cameraTarget.SetTarget(OwnerTransform);
    }
}
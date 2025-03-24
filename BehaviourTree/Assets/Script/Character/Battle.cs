using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Battle : MonoBehaviour
{
    [SerializeField] Status myInfo;
    public List<Status> curTargets;

    //공격 횟수
    public int AttackCombo = 0;

    //Battle Effect
    [SerializeField] GameObject AttackEffectSource;
    [SerializeField] GameObject SkillEffectSource;
    EffectController AttackEffect;
    EffectController SkillEffect;

    //Target Delegate
    public delegate Status[] GetTarget(PlayerType playerType);
    public GetTarget getTarget;

    //Camera Target Delegate
    public CameraTarget cameraTarget;

    //TurnIndigate
    public GameObject turnIndigate;

    Vector3 EffectStartPoint;

    private void Start()
    {
        AttackEffect = Instantiate(AttackEffectSource).GetComponent<EffectController>();
        SkillEffect = Instantiate(SkillEffectSource).GetComponent<EffectController>();
    }

    public void GetBaseAttackTarget()
    {
        curTargets.Clear();
        Status[] infors = null;

        //상대 진영의 List를 반환
        infors = getTarget(myInfo.playerType);

        curTargets.Add(infors[Random.Range(0, infors.Length - 1)]);
        EffectStartPoint = curTargets[0].startPos;

        if(myInfo.heroseDate.AttackType == AttackType.LONG)
        {
            cameraTarget.SetPosition(Vector3.Lerp(curTargets[0].transform.position, transform.position, 0.5f));
        }
        else
        {
            cameraTarget.SetTarget(transform);
        }

         CameraManager.Instance.setBattleMode = true;
    }

     public void Attack()
     {
        if (AttackCombo != 0)
        {
            curTargets[0].Hurt(myInfo.GetStatValue(Stat.STR) / 2);
        }
        else
        {
            curTargets[0].Hurt(myInfo.GetStatValue(Stat.STR));
        }
     }

    public void GetSkillTarget()
    {
        curTargets.Clear();
        //skill 사용하는 Player정보
        SkillData skillData = myInfo.skillDatas[myInfo.curSkillIndex];

        //타켓설정 변수들
        List<Status> oppositeCamps = new();

        //타켓 진영 및 범위를 지정하는 함수
        oppositeCamps = CheckArea(skillData);

        //우선순위를 통한 타켓 선정.
        switch (skillData.TargetPriority)
        {
            case TARGETPRIORITY.None:
                curTargets = oppositeCamps;
                break;
            case TARGETPRIORITY.Random:
                curTargets.Add(oppositeCamps[Random.Range(0, oppositeCamps.Count - 1)]);
                break;
            case TARGETPRIORITY.Hight_HP:
                {
                    int hight_HP = oppositeCamps[0].runTimeStat.CurHP;
                    int index = 0;

                    for (int i = 1; i < oppositeCamps.Count; ++i)
                    {
                        if (oppositeCamps[i].runTimeStat.CurHP > hight_HP)
                        {
                            hight_HP = oppositeCamps[i].runTimeStat.CurHP;
                            index = i;
                        }
                    }

                    curTargets.Add(oppositeCamps[index]);
                }
                break;
            case TARGETPRIORITY.Low_HP:
                {
                    int low_HP = oppositeCamps[0].runTimeStat.CurHP;
                    int index = 0;

                    for (int i = 1; i < oppositeCamps.Count; ++i)
                    {
                        if (oppositeCamps[i].runTimeStat.CurHP < low_HP)
                        {
                            low_HP = oppositeCamps[i].runTimeStat.CurHP;
                            index = i;
                        }
                    }

                    curTargets.Add(oppositeCamps[index]);
                }
                break;
            default:
                break;
        }
    }

    public List<Status> CheckArea(SkillData skillData)
    {
        List<Status> oppositeCamps = new();

        //타켓 진영을 선택
        if (skillData.TargetPlayerType == PlayerType.Enemy)
            oppositeCamps.AddRange(getTarget(myInfo.playerType));
        else
            oppositeCamps.AddRange(myInfo.playerType == PlayerType.Player ? getTarget(PlayerType.Enemy) : getTarget(PlayerType.Player));

        //타켓 범위를 지정
        switch (skillData.TargetRange)
        {
            case TargetArea.Self:
                {
                    List<Status> tempInfos = new();
                    tempInfos.Add(myInfo);
                    oppositeCamps = tempInfos;
                }
                break;

            case TargetArea.FrontRow:
                {
                    List<Status> tempInfos = new();

                    foreach (Status info in oppositeCamps)
                    {
                        if (info.charcterArea == Area.Front) tempInfos.Add(info);
                    }

                    if (tempInfos.Count != 0) oppositeCamps = tempInfos;
                }
                break;

            case TargetArea.BackRow:
                {
                    List<Status> tempInfos = new();

                    foreach (Status info in oppositeCamps)
                    {
                        if (info.charcterArea == Area.Back) tempInfos.Add(info);
                    }

                    if (tempInfos.Count != 0) oppositeCamps = tempInfos;
                }
                break;
            default:
                break;
        }
        return oppositeCamps;
    }

    public void UseSkill()
    {
        //다음게임으로 진행이 불가능 할 시 return;
        if (BattleManager.Instance.PossibleNextGame() == false)
            return;

        for (int i = 0; i < myInfo.skillDatas.Count; ++i)
        {
            SkillData skillData = myInfo.skillDatas[i];

            switch (skillData.SkillType)
            {
                case SKILLTYPE.ATTACK:
                    SkillAttack(skillData);
                    break;
                case SKILLTYPE.HELL:
                    SKillHeal(skillData);
                    break;
                case SKILLTYPE.BUFF:
                    SkillBuff(skillData);
                    break;
            }
        }
    }

    //SkillAction Delegate키워드 사용
    public void SkillAttack(SkillData skillData)
    {
        int Damage = myInfo.GetSkillValue();

        //스킬 사용자. 데미지관련 버프 Check
        if (myInfo.GetBuffValue(Stat.Damage) != 0)
        {
            Damage += (int)((float)Damage * ((float)myInfo.GetBuffValue(Stat.Damage) / 100.0f));
        }

        //피격 당하는 대상. 데미지 관련 버프 Check
        foreach (Status target in curTargets)
        {
            if (!target.IsDead)
            {
                target.Hurt(Damage);
            }
        }
    }

    //SkillAction Delegate키워드 사용
    public void SKillHeal(SkillData skillData)
    {
        foreach (Status target in curTargets)
        {
            target.Heal(myInfo.GetSkillValue());
        }
    }

    //SkillAction Delegate키워드 사용
    public void SkillBuff(SkillData skillData)
    {
        BuffSkillData buffData = (BuffSkillData)skillData;

        foreach (Status target in curTargets)
        {
            int buffValue = 0;

            switch (skillData.BonusStatType)
            {
                case Stat.None:
                    buffValue = (int)skillData.BonusStatValue;
                    break;
                default:
                    buffValue = (int)((float)myInfo.GetStatValue(skillData.BonusStatType) * skillData.BonusStatValue / 100.0f);
                    break;
            }

            Buff skillbuff = buffData.GetBuff;
            MyBuff buff = new MyBuff(skillbuff.buffType, skillbuff.buffStat, skillbuff.duration, buffValue, skillbuff.buffIcon);

            target.AddBuff(buff);
        }
    }

    //애니메이션 이벤트함수
    public void Action()
    {
        if (myInfo.UseSkill)
        {
            UseSkill();
        }
        else
        {
            Attack();
        }
    }

    //애니메이션 이벤트함수
    public void BattleEffectEmerge()
    {
        if (myInfo.UseSkill)
        {
            GetSkillEffectPosition();

            if (SkillEffect.isActiveAndEnabled == false)
            {
                SkillEffect.StartEffect(EffectStartPoint, transform.forward);
            }
            else
            {
                SkillEffect.ReStart();
            }

            if (myInfo.skillDatas[myInfo.curSkillIndex].SkillType == SKILLTYPE.ATTACK)
            {
                CameraManager.Instance.CameraShacking(0.15f, 1.2f);
            }
        }
        else
        {
            if (AttackEffect.isActiveAndEnabled == false)
            {
                AttackEffect.StartEffect(EffectStartPoint, transform.forward);
            }
            else
            {
                AttackEffect.ReStart();
            }

            CameraManager.Instance.CameraShacking(0.15f, 0.8f);
        }
    }

    void GetSkillEffectPosition()
    {
        SkillData curSkillData = myInfo.skillDatas[myInfo.curSkillIndex];

        if (curSkillData.SkillType == SKILLTYPE.ATTACK)
        {
            //대상이 하나
            if (curSkillData.TargetRange == TargetArea.Single)
            {
                EffectStartPoint = myInfo.heroseDate.AttackType == AttackType.CLOSE ? transform.position : curTargets[0].startPos;
                cameraTarget.SetTarget(transform);
            }
            else
            {
                //우리팀이 상대방을 공격
                if (curSkillData.TargetRange == TargetArea.FrontRow)
                {
                    EffectStartPoint = myInfo.playerType == PlayerType.Player ? Define.EnemyFrontPosition : Define.TeamFrontPosition;
                }
                else if (curSkillData.TargetRange == TargetArea.BackRow)
                {
                    EffectStartPoint = myInfo.playerType == PlayerType.Player ? Define.EnemyBackPosition : Define.TeamBackPosition;
                }
                else
                {
                    EffectStartPoint = myInfo.playerType == PlayerType.Player ? Define.EnemyMidPosition : Define.TeamMidPosition;
                }

                cameraTarget.SetPosition(Vector3.Lerp(transform.position, EffectStartPoint, 0.8f));
            }
        }
        else
        {
            if (curSkillData.TargetRange == TargetArea.Single)
            {
                EffectStartPoint = curTargets[0].startPos;
            }
            else
            {
                EffectStartPoint = myInfo.playerType == PlayerType.Player ? Define.TeamMidPosition : Define.EnemyMidPosition;
            }
        }
    }
}

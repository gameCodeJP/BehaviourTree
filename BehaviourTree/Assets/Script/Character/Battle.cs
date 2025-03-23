using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Battle : MonoBehaviour
{
    [SerializeField] Character myInfo;
    public List<Character> targets;

    //공격 횟수
    public int AttackCombo = 0;

    //Battle Effect
    [SerializeField] GameObject AttackEffectSource;
    [SerializeField] GameObject SkillEffectSource;
    EffectController AttackEffect;
    EffectController SkillEffect;

    //Target Delegate
    public delegate Character[] GetTarget(PlayerType playerType);
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
        targets.Clear();
        Character[] infors = null;

        //상대 진영의 List를 반환
        infors = getTarget(myInfo.playerType == PlayerType.Player ? PlayerType.Enemy : PlayerType.Player);

        //////////////////////////////////////////////
        ///우선도를 정하는 함수 구현 예정(Delegate를 활용하여 구현해보자)
        //////////////////////////////////////////////
        targets.Add(infors[Random.Range(0, infors.Length - 1)]);
        EffectStartPoint = targets[0].startPos;

        if(myInfo.heroseDate.AttackType == AttackType.LONG)
        {
            //Camera.main.GetComponent<CameraController>().CameraBattleMode(true, targets[0].transform);
            cameraTarget.SetPosition(Vector3.Lerp(targets[0].transform.position, transform.position, 0.5f));
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
            targets[0].Hurt(myInfo.GetStatValue(Stat.STR) / 2);
        }
        else
        {
            targets[0].Hurt(myInfo.GetStatValue(Stat.STR));
        }

         //Playlog.text = $"{attackInfo.heroseDate.HeroseName}가 {targetObj[0].heroseDate.HeroseName}에게 {attackInfo.GetStatValue(Stat.STR)}의 기본 공격을 함.";

         //0보다 작을 시 사망 처리
         /*if (targetObj[0].runTimeStat.CurHP <= 0)
         {
             DeadChracter(targetObj[0].ID);
         }*/
     }

    public void GetSkillTarget()
    {
        targets.Clear();
        //skill 사용하는 Player정보
        SkillData skillData = myInfo.skillDatas[myInfo.curSkillIndex];

        //타켓설정 변수들
        List<Character> oppositeCamps = new();

        //타켓 진영 및 범위를 지정하는 함수
        oppositeCamps = CheckArea(skillData);

        //우선순위를 통한 타켓 선정.
        switch (skillData.TargetPriority)
        {
            case TARGETPRIORITY.None:
                targets = oppositeCamps;
                break;
            case TARGETPRIORITY.Random:
                targets.Add(oppositeCamps[Random.Range(0, oppositeCamps.Count - 1)]);
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

                    targets.Add(oppositeCamps[index]);
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

                    targets.Add(oppositeCamps[index]);
                }
                break;
            default:
                break;
        }
        //GetSkillEffectPosition();
    }

    public List<Character> CheckArea(SkillData skillData)
    {
        List<Character> oppositeCamps = new();

        //타켓 진영을 선택
        if (skillData.TargetPlayerType == PlayerType.Enemy)
            oppositeCamps.AddRange(myInfo.playerType == PlayerType.Player ? getTarget(PlayerType.Enemy): getTarget(PlayerType.Player));
        else
        oppositeCamps.AddRange(myInfo.playerType == PlayerType.Player ? getTarget(PlayerType.Player) : getTarget(PlayerType.Enemy));

        //타켓 범위를 지정
        switch (skillData.TargetRange)
        {
            case TargetArea.Self:
                {
                    List<Character> tempInfos = new();
                    tempInfos.Add(myInfo);
                    oppositeCamps = tempInfos;
                }
                break;

            case TargetArea.FrontRow:
                {
                    List<Character> tempInfos = new();

                    foreach (Character info in oppositeCamps)
                    {
                        if (info.charcterArea == Area.Front) tempInfos.Add(info);
                    }

                    if (tempInfos.Count != 0) oppositeCamps = tempInfos;
                }
                break;

            case TargetArea.BackRow:
                {
                    List<Character> tempInfos = new();

                    foreach (Character info in oppositeCamps)
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
        if (!GameManager.Instance.PossibleNextGame())
        {
            return;
        }

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
        foreach (Character target in targets)
        {
            if (!target.IsDead)
            {
                target.Hurt(Damage);
            }
        }

        //targetObjs => { Hurt(Damage, deadChracterFuntion); }

        //Playlog.text = $"{useSkillObj.heroseDate.HeroseName}가 {Damage}데미지의 스킬 공격";
    }

    //SkillAction Delegate키워드 사용
    public void SKillHeal(SkillData skillData)
    {
        foreach (Character target in targets)
        {
            target.Heal(myInfo.GetSkillValue());
            /*if (target.runTimeStat.CurHP + myInfo.GetSkillValue() > target.runTimeStat.MaxHP) target.runTimeStat.CurHP = target.runTimeStat.MaxHP;
            else target.runTimeStat.CurHP += myInfo.GetSkillValue();*/
        }

        //Playlog.text = $"{useSkillObj.heroseDate.HeroseName}가 {useSkillObj.GetSkillValue()}의 힐을 사용";
    }

    //SkillAction Delegate키워드 사용
    public void SkillBuff(SkillData skillData)
    {
        BuffSkillData buffData = (BuffSkillData)skillData;

        foreach (Character target in targets)
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

            //Playlog.text = $"{useSkillObj.heroseDate.HeroseName}가 버프스킬을 사용.";

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
                EffectStartPoint = myInfo.heroseDate.AttackType == AttackType.CLOSE ? transform.position : targets[0].startPos;
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
                EffectStartPoint = targets[0].startPos;
            }
            else
            {
                EffectStartPoint = myInfo.playerType == PlayerType.Player ? Define.TeamMidPosition : Define.EnemyMidPosition;
            }
        }
    }
}

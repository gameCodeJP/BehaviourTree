using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using Unity.VisualScripting;

[DefaultExecutionOrder(-1)]
public class Character : MonoBehaviour
{
    public HeroseStatData heroseDate;
    public List<SkillData> skillDatas;

    public PlayerType playerType;
    public RunTimeStat runTimeStat;

    public List<MyBuff> buffs = new ();

    // 고유 번호
    public int ID = 0;
    enum Gender { Man,Girl}
    [SerializeField] Gender gender;

    // 자신의위치
    public Vector3 startPos;
    public Quaternion startRotation;

    // 자기 자리의 인덱스번호
    public int indexNum = 0;

    public bool IsHurt = false;
    public bool IsDead = false;
    public bool OnUpdate = true;
    public bool UseSkill = false;

    // 현재 실행중인 SkillIndex
    public int curSkillIndex = 0;

    // 자신의 위치
    public Area charcterArea;

    // Buff
    public GameObject buffUI_Base;
    [SerializeField] BuffManager buffUiManager;

    // Event
    public UnityEvent DeadEvent = new();

    // Animator
    private Animator playerAnimator;

    //Victory
    public bool victory = false;

    private void Start()
    {
        // 교체 예정
        startPos = transform.position;

        // 가지고 있는 정보를 토대로 스탯을 수정
        SettingRunTimeStat();
        playerAnimator = GetComponent<Animator>();
    }

    public void AddDeadEvent(UnityAction unityAction)
    {
        DeadEvent.AddListener(unityAction);
    }

    //스태틱값들을 미리 계산
    public void SettingRunTimeStat()
    {
        runTimeStat = new RunTimeStat(heroseDate.HP, heroseDate.MP, heroseDate.STR, heroseDate.INT, heroseDate.AGI, heroseDate.VLT, heroseDate.LUK);

        /////////////////////////////////////////////////////////
        ///레벨 계산
        /////////////////////////////////////////////////////////     

        //////////////////////////////////////////////////////////
        ///아이템 효과 계산
        //////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////
        ///Acitive Skill 계산
        //////////////////////////////////////////////////////////
    }

    public int GetStatValue(Stat stat)
    {
        int finalValue = 0;

        switch (stat)
        {
            case Stat.STR:
                finalValue = heroseDate.STR;
                break;
            case Stat.INT:
                finalValue = heroseDate.INT;
                break;
            case Stat.AGI:
                finalValue = heroseDate.AGI;
                break;
            case Stat.VLT:
                finalValue = heroseDate.VLT;
                break;
            case Stat.LUK:
                finalValue = heroseDate.LUK;
                break;
        }

        finalValue += GetBuffValue(stat);

        return finalValue;
    }

    public int GetSkillValue()
    {
        int Value = GetStatValue(skillDatas[curSkillIndex].BonusStatType) +
            (int)((float)GetStatValue(skillDatas[curSkillIndex].BonusStatType) * (skillDatas[curSkillIndex].BonusStatValue / 100));

        Value += GetBuffValue(Stat.Damage);

        return Value;
    }

    public void AddBuff(MyBuff buff)
    {
        buffs.Add(buff);

        //UI추가
        BuffUI buffUI = Instantiate(buffUI_Base, buffUiManager.transform).GetComponent<BuffUI>();
        buffUI.Setting(buff);
        buffUiManager.AddBuffUI(buffUI);
    }

    //버프 Duration감소 및 Check
    public void BuffCheck()
    {
        for(int i = 0; i < buffs.Count; ++i)
        {
            if(buffs[i].buffType == BuffType.TurnDamage)
            {
                Hurt(buffs[i].value);
            }

            //Duration이 0일시 true반환
            if(buffs[i].DurationCheck())
            {
                buffs.RemoveAt(i);
                --i;
            }
        }

        //buffUiCheck
        buffUiManager.BuffsDurationCheck();
    }

    public int GetBuffValue(Stat stat)
    {
        int buffValue = 0;

        for (int i = 0; i < buffs.Count; ++i)
        {
            if (buffs[i].buffStat != stat)
                continue;

            switch (buffs[i].buffType)
            {
                case BuffType.Buff:
                    buffValue += buffs[i].value;
                    break;
                case BuffType.DeBuff:
                    buffValue -= buffs[i].value;
                    break;
            }
        }

        return buffValue;
    }

    public void Hurt(int damage)
    {
        if (IsDead == true) 
            return;

        IsHurt = true;
        //방어력 검사 및 데미지 감소 버프들 확인
        damage -= (GetStatValue(Stat.AGI) / 5) + (int)((float)damage * (GetBuffValue(Stat.Damage) / 100.0f));

        if (damage > 0) runTimeStat.CurHP -= damage;

        if (runTimeStat.CurHP <= 0)
        {
            DeadEvent.Invoke();
        }
        else
        {
            if (!playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
            {
                playerAnimator.Play("Hurt");
            }
            else
            {
                playerAnimator.Play("Hurt", 0, 0);
            }
        }

        EffectManager.Instance.TriggerEffect(EffectName.Attack, transform.position, transform.forward);
        EffectManager.Instance.TriggerDamageEffect(SKILLTYPE.ATTACK, transform.position, transform.forward, damage);
    }

    public void Heal(int healValue)
    {
        runTimeStat.CurHP = runTimeStat.CurHP + healValue > runTimeStat.MaxHP ? runTimeStat.MaxHP : runTimeStat.CurHP + healValue;
        EffectManager.Instance.TriggerDamageEffect(SKILLTYPE.HELL, transform.position, transform.forward, healValue);
    }

    //애니메이션 이벤트로 호출함
    public void HurtRelese()
    {
        IsHurt = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerType { Player, Enemy }
public enum TargetType { BaseAttack, Skill }; [System.Serializable]

public enum Area { Front, Back }
public struct RunTimeStat
{
    public RunTimeStat(int base_HP, int base_MP, int base_STR, int base_INT, int base_AGI, int base_VLT, int base_LUK)
    {
        MaxHP = base_HP;
        CurHP = base_HP;
        MaxMP = base_MP;
        CurMP = 0;
        STR = base_STR;
        INT = base_INT;
        AGI = base_AGI;
        VLT = base_VLT;
        LUK = base_LUK;
    }

    public int MaxHP;
    public int CurHP;
    public int MaxMP;
    public int CurMP;
    public int STR;
    public int INT;
    public int AGI;
    public int VLT;
    public int LUK;
}

public class MyBuff
{
    public BuffType buffType;
    public Stat buffStat;
    public int duration;
    public int value;
    public Sprite icon;

    public MyBuff(BuffType _buffType, Stat _buffStat, int _duration, int _value, Sprite _icon)
    {
        buffType = _buffType;
        buffStat = _buffStat;
        duration = _duration;
        value = _value;
        icon = _icon;
    }

    public bool DurationCheck()
    {
        return --duration == 0;
    }
}
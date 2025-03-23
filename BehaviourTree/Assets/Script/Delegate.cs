using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DelegateFuction
{
    public enum TargetType { BaseAttack, Skill };

    //Delegate
    public delegate void SkillAction(Information useSkillObj, Information[] targetObjs, SkillData skillData);
    public delegate void DeadChracter(int playerNum);
    public delegate void AddReadyChracter(HeroseName heroseName);
    public delegate void CreateChracter(HeroseName heroseName);
}

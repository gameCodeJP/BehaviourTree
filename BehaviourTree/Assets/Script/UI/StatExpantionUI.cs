using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatExpantionUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI StatText;
    GameObject target = null;
    Information targetInfo = null;

    public void SetStatExplanation(int num) 
    {
        targetInfo = BattleManager.Instance.allInformations[num];

        if (!targetInfo.IsDead)
        {
            target = targetInfo.gameObject;
            StatText.text = $"name : {targetInfo.heroseDate.HeroseName} \n" +
                $"HP : {targetInfo.runTimeStat.CurHP} / {targetInfo.runTimeStat.MaxHP} \n" +
                $"MP : {targetInfo.runTimeStat.CurMP} / { targetInfo.runTimeStat.MaxMP} \n" +
                $"STR : {targetInfo.runTimeStat.STR} + {targetInfo.GetBuffValue(Stat.STR)}\n" +
                $"INT : {targetInfo.runTimeStat.INT} + {targetInfo.GetBuffValue(Stat.INT)}\n" +
                $"AGI : {targetInfo.runTimeStat.AGI} + {targetInfo.GetBuffValue(Stat.AGI)}\n" +
                $"LUK : {targetInfo.runTimeStat.LUK} + {targetInfo.GetBuffValue(Stat.LUK)}\n";
        }
    }

    private void LateUpdate()
    {
        var wantedPos = Camera.main.WorldToScreenPoint(target.transform.position);
        transform.position = wantedPos + new Vector3(150, 200, 0);
    }
}

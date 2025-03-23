using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Update : MonoBehaviour
{
    [SerializeField] Character ownerInfo;
    [SerializeField] Image HP_Bar;
    [SerializeField] Image MP_Bar;
    [SerializeField] TextMeshProUGUI heroName;

    private void Start()
    {
        heroName.text = ownerInfo.heroseDate.HeroseName.ToString();
        ownerInfo.AddDeadEvent(() => gameObject.SetActive(false));
    }

    private void Update()
    {
        HP_Bar.fillAmount = (float)ownerInfo.runTimeStat.CurHP / (float)ownerInfo.runTimeStat.MaxHP;
        MP_Bar.fillAmount = (float)ownerInfo.runTimeStat.CurMP / (float)ownerInfo.runTimeStat.MaxMP;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ChracterChoiceUI : MonoBehaviour
{
    [SerializeField] Image chracterSprite;
    [SerializeField] Text chracterName;
    private HeroseName heroseName;
    private UnityAction<HeroseName> choiceChracterEvent;

    //선택중인지를 판단할때 사용
    bool OnChoice;

    public void Setting(HoldHeros holdHeros, UnityAction<HeroseName> unityAction)
    {
        HeroseStatData heroseStatData = DataManager.Instance().GetHeroseStatData((int)holdHeros.heroseName);
        
        chracterSprite.sprite = heroseStatData.HeroSprite;
        heroseName = heroseStatData.HeroseName;
        chracterName.text = heroseStatData.HeroseName.ToString();
        choiceChracterEvent = unityAction;
    }

    public void ClivkEvent()
    {
        if (OnChoice)
            return;

        choiceChracterEvent(heroseName);
        OnChoice = true;
    }
}

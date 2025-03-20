using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterChoiceCenter : MonoBehaviour
{
    [SerializeField] GameObject characterChoiceUI;

    private void Awake()
    {
        HoldHeros[] holdHeros = DataManager.Instance().getHoldHeros;

        for (int i = 0; i < holdHeros.Length; ++i)
        {
            ChracterChoiceUI choiceUI = Instantiate(characterChoiceUI, this.transform).GetComponent<ChracterChoiceUI>();
            choiceUI.Setting(holdHeros[i], ReadySenter.Instance().AddReadyCharacter);
        }
    }
}

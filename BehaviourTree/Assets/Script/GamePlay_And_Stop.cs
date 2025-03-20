using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlay_And_Stop : MonoBehaviour
{
    [SerializeField] Text TextUI;
    bool GamePlay = true;
    string Play = "Play";
    string Stop = "Stop";

    public void GamePlayOrStop()
    {
        GamePlay = !GamePlay;
        BattleManager.Instance.GamePlayAndStop(GamePlay);
        TextUI.text = GamePlay == true ? Stop : Play;
    }
}

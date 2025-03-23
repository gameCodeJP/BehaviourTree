using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    private static GameManager _Instance;
    public static GameManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType(typeof(GameManager)) as GameManager;
            }

            return _Instance;
        }
    }

    enum TRUN { Player, Enemy }

    public int turn;
    public int TurnChracter;

    public int playerCount = 5;
    public int enemyCount = 5;

    //PlayerData
    List<Character> playerInfos;
    [SerializeField] List<Character> enemyInfos;
    public List<Character> allInformations = new();
    public Queue<int> turnPreferentially = new Queue<int>();

    //UI
    public ExpantionUI expantionUI;
    public StatExpantionUI statExpantionUI;
    [SerializeField] Text Playlog;
    [SerializeField] GameObject RestartButton;
    [SerializeField] TurnCheck_UI turnCheckUI;

    public GameObject winUI;
    public GameObject loseUI;

    //EffectManager
    [SerializeField] DamageEffectUI_Senter damageEffectUI_Senter;

    [SerializeField] float TurnDelay;

    //CameraTarget
    [SerializeField] CameraTarget cameraTarget;

    public void GameStart(List<Character> playerCharacters)
    {
        int ID = 0;

        //고유 번호 할당 및 playerType 지정
        playerInfos = playerCharacters;
        foreach (Character playerInfo in playerInfos)
        {
            if (playerInfo == null)
                continue;

            playerInfo.playerType = PlayerType.Player;
            RegisterFunction(playerInfo, ID++);
        }

        foreach (Character enemyInfo in enemyInfos)
        {
            if (enemyInfo == null)
                continue;

            enemyInfo.playerType = PlayerType.Enemy;
            RegisterFunction(enemyInfo, ID++);
        }

        allInformations.AddRange(playerInfos);
        allInformations.AddRange(enemyInfos);

        playerCount = playerInfos.Count;
        enemyCount = enemyInfos.Count;

        GamePlayAndStop(true);

        NextTurn();
    }

    private void RegisterFunction(Character info, int id)
    {
        info.AddDeadEvent(() => DeadChracter(id));

        //BattleDelegate설정
        info.ID = id;
        Battle BattleComponent = info.GetComponent<Battle>();
        BattleComponent.getTarget = GetTargets;
        BattleComponent.cameraTarget = cameraTarget;

        info.GetComponent<CharacterState>().CurState = State.Battle;
    }

    public void WaitTurn()
    {
        StartCoroutine(WaitTurnCorountine());
    }

    IEnumerator WaitTurnCorountine()
    {
        yield return new WaitForSeconds(2.0f);
        NextChracter();
    }

    private void NextTurn()
    {
        ++turn;
        turnCheckUI.NextTurn();

        //buff Check구간
        foreach (Character info in allInformations)
        {
            if(!info.IsDead)
            info.BuffCheck();
        }

        //게임 진행이 가능한지 Check
        if (PossibleNextGame())
        {
            SettingPreferentially();
            NextChracter();
        }
        else GameOver();
    }

    public void NextChracter()
    {
        //양팀에 한명이라도 남아 있어야지 게임진행이 가능
        if (PossibleNextGame())
        {
            //모든 순서가 끝나면 다음 턴으로 진행
            while (turnPreferentially.Count != 0)
            {
                int NextChracterNum = turnPreferentially.Dequeue();

                //죽은 객체라면 다음 순서로 넘김
                if (allInformations[NextChracterNum].IsDead)
                {
                    continue;
                }
                else
                {
                    StartCoroutine(TrunDelayCoroutine(NextChracterNum));
                    return;
                }
            }
        }

        /////////////////////////////////////////////
        ///순서를 기다리게 할 함수 구현예정
        /////////////////////////////////////////////
 
        NextTurn();
    }

    public bool PossibleNextGame()
    {
        //한쪽 진영 전멸 시 게임종료
        if (playerInfos.Count <= 0 || enemyInfos.Count <= 0) return false;

        return true;
    }

    public void SettingPreferentially()
    {
        //LUK를 통한 순서 결정
        //오름차순으로 정렬
        Character[] Infos = allInformations.ToArray();


        for (int i = 0; i < Infos.Length - 1; ++i)
        {
            if (Infos[i].GetStatValue(Stat.LUK) < Infos[i + 1].GetStatValue(Stat.LUK))
            {
                Character tempinfo = Infos[i];
                Infos[i] = Infos[i + 1];
                Infos[i + 1] = tempinfo;

                i = 0;
                continue;
            }
        }

        foreach (Character info in Infos)
        {
            turnPreferentially.Enqueue(info.ID); 
        }
    }

    void GameOver()
    {
        foreach (Character info in allInformations)
        {
            if (!info.IsDead)
            {
                info.victory = true;
            }
        }

        if (playerInfos.Count == 0) loseUI.SetActive(true);
        else winUI.SetActive(true);

        RestartButton.SetActive(true);
    }

    public void GamePlayAndStop(bool on)
    {
        foreach (Character info in allInformations)
        {
            info.OnUpdate = on;
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public Character[] GetTargets(PlayerType playertype)
    {
        return playertype == PlayerType.Player ? playerInfos.ToArray() : enemyInfos.ToArray();
    }

    public void DeadChracter(int characterNum)
    {
        Character deadCharacter = allInformations[characterNum];
        deadCharacter.IsDead = true;

        //해당 진영 List에서 삭제
        if (deadCharacter.playerType == PlayerType.Player)
        {
            --playerCount;
            playerInfos.Remove(deadCharacter);
        }
        else
        {
            --enemyCount;
            enemyInfos.Remove(deadCharacter);
        }

        //Destroy(deadCharacter);
    }

    IEnumerator TrunDelayCoroutine(int NextChracterNum)
    {
        GamePlayAndStop(false);
        Camera.main.GetComponent<CameraManager>().CameraBattleMode(false);
        yield return new WaitForSeconds(TurnDelay);

        //Camera.main.GetComponent<CameraController>().CameraBattleMode(true, allInformations[NextChracterNum].transform);
        TurnChracter = NextChracterNum;
        GamePlayAndStop(true);
    }
}
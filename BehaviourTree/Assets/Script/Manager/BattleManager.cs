using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{

    private static BattleManager _Instance;
    public static BattleManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType(typeof(BattleManager)) as BattleManager;
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
    [SerializeField] List<Information> playerInfors;
    [SerializeField] List<Information> enemyInfors;
    public List<Information> allInformations = new();
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
    [SerializeField] EffectManager effectManager;
    [SerializeField] DamageEffectUI_Senter damageEffectUI_Senter;

    [SerializeField] float TurnDelay;

    //CameraTarget
    [SerializeField] CameraTarget cameraTarget;

    public void GameStart(List<Information> playerCharacters)
    {
        //준비된 정보받기
        playerInfors = playerCharacters;

        int ID = 0;

        //고유 번호 할당 및 playerType 지정
        foreach (Information playerInfor in playerInfors)
        {
            if (playerInfor == null)
                continue;

            playerInfor.playerType = PlayerType.Player;
            playerInfor.useEffect = effectManager.EffectEmerge;
            playerInfor.useDamageValueEffect = damageEffectUI_Senter.DamageValueEffectEmerge;
            playerInfor.playerDead = DeadChracter;

            //Delegate설정
            Battle BattleComponent = playerInfor.GetComponent<Battle>();
            BattleComponent.getTarget = GetTargets;
            BattleComponent.cameraShaking = Camera.main.GetComponent<CameraController>().CameraShacking;
            BattleComponent.battleMode = Camera.main.GetComponent<CameraController>().CameraBattleMode;
            BattleComponent.cameraTarget = cameraTarget;

            playerInfor.GetComponent<CharacterState>().CurState = State.Battle;

            playerInfor.ID = ID;
            ++ID;
        }

        while (playerInfors.Remove(null)) ;

        for (int i = 0; i < 5; ++i)
        {
            if (enemyInfors[i] == null)
                continue;

            enemyInfors[i].startRotation = Quaternion.Euler(0 , 90, 0);
            enemyInfors[i].transform.localRotation = Quaternion.Euler(0, 90, 0);
            enemyInfors[i].gameObject.SetActive(true);
            enemyInfors[i].playerType = PlayerType.Enemy;

            //Delegate 설정
            enemyInfors[i].useEffect = effectManager.EffectEmerge;
            enemyInfors[i].useDamageValueEffect = damageEffectUI_Senter.DamageValueEffectEmerge;
            enemyInfors[i].playerDead = DeadChracter;

            //BattleDelegate설정
            Battle BattleComponent = enemyInfors[i].GetComponent<Battle>();
            BattleComponent.getTarget = GetTargets;
            BattleComponent.cameraShaking = Camera.main.GetComponent<CameraController>().CameraShacking;
            BattleComponent.battleMode = Camera.main.GetComponent<CameraController>().CameraBattleMode;
            BattleComponent.cameraTarget = cameraTarget;

            enemyInfors[i].GetComponent<CharacterState>().CurState = State.Battle;

            if (i < 2) enemyInfors[i].charcterArea = Area.Front;
            else enemyInfors[i].charcterArea = Area.Back;

            enemyInfors[i].ID = ID;
            ++ID;
        }

        allInformations.AddRange(playerInfors);
        allInformations.AddRange(enemyInfors);

        playerCount = playerInfors.Count;
        enemyCount = enemyInfors.Count;

        GamePlayAndStop(true);

        NextTurn();
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
        foreach (Information info in allInformations)
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
        if (playerInfors.Count <= 0 || enemyInfors.Count <= 0) return false;

        return true;
    }

    public void SettingPreferentially()
    {
        //LUK를 통한 순서 결정
        //오름차순으로 정렬
        Information[] Infos = allInformations.ToArray();


        for (int i = 0; i < Infos.Length - 1; ++i)
        {
            if (Infos[i].GetStatValue(Stat.LUK) < Infos[i + 1].GetStatValue(Stat.LUK))
            {
                Information tempinfo = Infos[i];
                Infos[i] = Infos[i + 1];
                Infos[i + 1] = tempinfo;

                i = 0;
                continue;
            }
        }

        foreach (Information info in Infos)
        {
            turnPreferentially.Enqueue(info.ID); 
        }
    }

    void GameOver()
    {
        foreach (Information info in allInformations)
        {
            if (!info.IsDead)
            {
                info.victory = true;
            }
        }

        if (playerInfors.Count == 0) loseUI.SetActive(true);
        else winUI.SetActive(true);

        RestartButton.SetActive(true);
    }

    public void GamePlayAndStop(bool on)
    {
        foreach (Information info in allInformations)
        {
            info.OnUpdate = on;
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public Information[] GetTargets(PlayerType playertype)
    {
        return playertype == PlayerType.Player ? playerInfors.ToArray() : enemyInfors.ToArray();
    }

    public void DeadChracter(int characterNum)
    {
        Information deadCharacter = allInformations[characterNum];
        deadCharacter.IsDead = true;

        //해당 진영 List에서 삭제
        if (deadCharacter.playerType == PlayerType.Player)
        {
            --playerCount;
            playerInfors.Remove(deadCharacter);
        }
        else
        {
            --enemyCount;
            enemyInfors.Remove(deadCharacter);
        }

        //Destroy(deadCharacter);
    }

    IEnumerator TrunDelayCoroutine(int NextChracterNum)
    {
        GamePlayAndStop(false);
        Camera.main.GetComponent<CameraController>().CameraBattleMode(false);
        yield return new WaitForSeconds(TurnDelay);

        //Camera.main.GetComponent<CameraController>().CameraBattleMode(true, allInformations[NextChracterNum].transform);
        TurnChracter = NextChracterNum;
        GamePlayAndStop(true);
    }
}
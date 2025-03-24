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

    public int playerCount;
    public int enemyCount;

    //PlayerData
    List<Status> playerStatus = new();
    [SerializeField] List<Status> enemyStatus = new();
    public List<Status> allStatus = new();
    public Queue<int> turnPreferentially = new Queue<int>();

    //UI
    public ExpantionUI expantionUI;
    public StatExpantionUI statExpantionUI;
    [SerializeField] GameObject restartButton;
    [SerializeField] TurnCheck_UI turnCheckUI;

    public GameObject winUI;
    public GameObject loseUI;

    //EffectManager
    [SerializeField] DamageEffectUI_Senter damageEffectUI_Senter;

    //CameraTarget
    [SerializeField] CameraTarget cameraTarget;

    [SerializeField] float turnDelay;

    public void GameStart(List<Status> playerStatusList)
    {
        int id = 0;

        playerStatus = playerStatusList;

        //고유 번호 할당 및 playerType 지정
        RegisterStatus(ref playerStatus, ref id, PlayerType.Player);
        RegisterStatus(ref enemyStatus, ref id, PlayerType.Enemy);

        playerCount = playerStatus.Count;
        enemyCount = enemyStatus.Count;

        GamePlayAndStop(true);

        NextTurn();
    }

    private void RegisterStatus(ref List<Status> statusList, ref int id, PlayerType playerType)
    {
        foreach (Status status in statusList)
        {
            if (status == null)
                continue;

            int localInt = id;

            status.ID = id;
            status.playerType = playerType;
            status.AddDeadEvent(() => DeadChracter(localInt));

            //BattleDelegate설정
            Battle BattleComponent = status.GetComponent<Battle>();
            BattleComponent.getTarget = GetTargets;
            BattleComponent.cameraTarget = cameraTarget;

            status.GetComponent<CharacterState>().CurState = State.Battle;

            ++id;
        }

        allStatus.AddRange(statusList);
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
        foreach (Status status in allStatus)
        {
            if (status.IsDead)
                continue;

            status.BuffCheck();
        }

        //게임 진행이 가능한지 Check
        if (PossibleNextGame() == false)
        {
            GameOver();
            return;
        }

        SettingPreferentially();
        NextChracter();
    }

    public void NextChracter()
    {
        //게임 진행이 가능한지 Check
        if (PossibleNextGame() == false)
        {
            NextTurn();
            return;
        }

        //모든 순서가 끝나면 다음 턴으로 진행
        while (turnPreferentially.Count != 0)
        {
            int nextId = turnPreferentially.Dequeue();
            if (allStatus[nextId].IsDead) //죽은 객체라면 다음 순서로 넘김
                continue;

            StartCoroutine(TrunDelayCoroutine(nextId));
            return;
        }
    }

    public bool PossibleNextGame()
    {
        //한쪽 진영 전멸 시 게임종료
        bool isPossible = (playerStatus.Count > 0 && enemyStatus.Count > 0);
        return isPossible;
    }

    public void SettingPreferentially()
    {
        //LUK를 통한 순서 결정
        //오름차순으로 정렬
        Status[] Infos = allStatus.ToArray();
        for (int i = 0; i < Infos.Length - 1; ++i)
        {
            if (Infos[i].GetStatValue(Stat.LUK) < Infos[i + 1].GetStatValue(Stat.LUK))
            {
                Status tempinfo = Infos[i];
                Infos[i] = Infos[i + 1];
                Infos[i + 1] = tempinfo;

                i = 0;
            }
        }

        foreach (Status info in Infos)
        {
            turnPreferentially.Enqueue(info.ID); 
        }
    }

    void GameOver()
    {
        foreach (Status info in allStatus)
        {
            if (info.IsDead == false)
            {
                info.victory = true;
            }
        }

        if (playerStatus.Count == 0)
        {
            loseUI.SetActive(true);
        }
        else
        {
            winUI.SetActive(true);
        }

        restartButton.SetActive(true);
    }

    public void GamePlayAndStop(bool on)
    {
        foreach (Status info in allStatus)
        {
            info.OnUpdate = on;
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public Status[] GetTargets(PlayerType playertype)
    {
        if (playertype == PlayerType.Player)
            return enemyStatus.ToArray();

        return playerStatus.ToArray();
    }

    public void DeadChracter(int id)
    {
        Status deadCharacter = allStatus[id];
        deadCharacter.IsDead = true;

        //해당 진영 List에서 삭제
        if (deadCharacter.playerType == PlayerType.Player)
        {
            --playerCount;
            playerStatus.Remove(deadCharacter);
        }
        else
        {
            --enemyCount;
            enemyStatus.Remove(deadCharacter);
        }
    }

    IEnumerator TrunDelayCoroutine(int nextId)
    {
        GamePlayAndStop(false);
        Camera.main.GetComponent<CameraManager>().CameraBattleMode(false);
        yield return new WaitForSeconds(turnDelay);

        TurnChracter = nextId;
        GamePlayAndStop(true);
    }
}
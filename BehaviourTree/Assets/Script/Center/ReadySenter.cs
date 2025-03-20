using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.MPE;
using UnityEngine;
using UnityEngine.Events;

public class ReadySenter : MonoBehaviour
{
    static private ReadySenter _instance;

    public static ReadySenter Instance()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType(typeof(ReadySenter)) as ReadySenter;
        }

        return _instance;
    }

    //캐릭터 Base
    [SerializeField] GameObject[] characterBase;

    [SerializeField] Transform[] startPos;
    List<Information> readyCharacters = new ();
    bool[] OnIndex = new bool[5];
    int characterCount = 0;

    //UI
    [SerializeField] GameObject CharacterScrollUI;

    [SerializeField] UnityEvent UIActiveEvent;

    public void AddReadyCharacter(HeroseName name)
    {
        // 최대 갯수 5캐릭터
        if (characterCount == 5)
            return;

        HeroseStatData heroseStatData = DataManager.Instance().GetHeroseStatData((int)name);
        //해당하는 prefab을 받는다.
        GameObject Character = Instantiate(characterBase[(int)heroseStatData.HeroseName]);
        Information CharacterInfo = Character.GetComponent<Information>();

        readyCharacters.Add(CharacterInfo);
        CharacterInfo.skillDatas.AddRange(DataManager.Instance().GetSkillDatas((int)name));

        //위치지정
        for (int i = 0; i < 5; ++i)
        {
            if (OnIndex[i] == false)
            {
                OnIndex[i] = true;
                CharacterInfo.transform.position = startPos[i].position;
                CharacterInfo.transform.localRotation = Quaternion.Euler(0, -90, 0);
                CharacterInfo.startRotation = Quaternion.Euler(0, -90, 0);
                CharacterInfo.indexNum = i;
                CharacterInfo.charcterArea = i < 2 ? Area.Front : Area.Back;
                break;
            }
        }

        ++characterCount;
    }

    public void ChracterChangePosition(int chracterNum, Transform changeStartPosObj)
    {
        //바꾸길 원하는 객체 검색
        Information info;
        info = readyCharacters[chracterNum];

        OnIndex[chracterNum] = false;
        readyCharacters[chracterNum] = null;

        int changeIndex = 0;
        for (int i = 0; i < 5; ++i) //바꾸고 싶은위치 찾기
        {
            if (startPos[i] == changeStartPosObj)
            {
                changeIndex = i;
                break;
            }
        }

        //객체가 있다면
        if (OnIndex[changeIndex])
        {
            PlaceChraccter(chracterNum, readyCharacters[changeIndex]);
        }

        PlaceChraccter(changeIndex, info);
    }

    public void PlaceChraccter(int idx, Information info)
    {
        OnIndex[idx] = true;
        readyCharacters[idx] = info;

        info.startPos = startPos[idx].position;
        info.transform.position = startPos[idx].position;

        info.GetComponent<MouseEvent>().indexNum = idx;
        info.indexNum = idx;
    }

    public void OnStartPosCollider()
    {
        for(int i = 0; i < startPos.Length; ++i)
        {
            startPos[i].GetComponent<BoxCollider>().enabled = true;
        }
    }

    public void OffStartPosCollider()
    {
        for (int i = 0; i < startPos.Length; ++i)
        {
            startPos[i].GetComponent<BoxCollider>().enabled = false;
        }
    }

    public void StartGame()
    {
        BattleManager.Instance.GameStart(readyCharacters);
        for(int i = 0; i < startPos.Length; ++i)
        {
            startPos[i].gameObject.SetActive(false);
        }
        UIActiveEvent.Invoke();
    }
}

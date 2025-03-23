using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseEvent : MonoBehaviour
{
    Vector3 StartPos;
    public CharacterState characterState;
    public int indexNum;
    public int CharacterNum = -1;

    private void Start()
    {
        StartPos = transform.position;
        indexNum = GetComponent<Information>().indexNum;
    }

    private void OnMouseDrag()
    {
        if (characterState.CurState == State.Ready)
        { 
            Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
            transform.position = point;
        }
    }

    private void OnMouseUp()
    {
        if (characterState.CurState == State.Ready)
            transform.position = StartPos;
    }

    private void OnMouseEnter()
    {
        if (CharacterNum == -1)
        {
            CharacterNum = GetComponent<Information>().ID;
        }

        /*if (characterState.CurState == State.Battle)
            OnInfomation();*/
    }

   /* private void OnMouseExit()
    {
        if (characterState.CurState == State.Battle)
            OffInfomation();
    }*/

    //StatUI호출 및 Setting
    public void OnInfomation()
    {
        BattleManager.Instance.statExpantionUI.SetStatExplanation(CharacterNum);
        BattleManager.Instance.statExpantionUI.gameObject.SetActive(true);
    }

    //StatUI비활성화
    public void OffInfomation()
    {
        BattleManager.Instance.statExpantionUI.gameObject.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Node : ScriptableObject
{ 
    private Information _information;
    private Transform _ownerTransform;
    public Battle _battle;
    public Animator playerAnimator;

    public Information Info { get { return _information; } set { _information = value; } }
    public Transform OwnerTransform { get { return _ownerTransform; } set { _ownerTransform = value; } }
    public Battle OwnerBattle { get { return _battle; } set { _battle = value; } }

    public enum State
    {
        Running,
        Failure,
        Success
    }

    public State state = State.Running;
    public bool started = false;
    public string guid;
    public Vector2 position;

    public State Update()
    {
        //이번 업데이트에서 검사를 실행한 적이 있는지를 확인
        if(!started)
        {
            //첫 검사 시 초기화를 한후 started를 true를 두어 2번 들어가지 않게 예외처리
            OnStart();
            started = true;
        }

        //자신이 State가 무엇일지 검사를 실행
        //자식을 가지고 있는 객체는 자식에 의해 State값이 달라진다.
        //자식이 없는 ActionNode인 경우 자신의 Update의 결과 값이 State가 된다.
        state = OnUpdate();

        //Running을 제외한 결과들은 노드의 활동을 멈춘다.
        if(state == State.Failure || state == State.Success)
        {
            OnStop();
            started = false;
        }

        return state;
    }

    public virtual Node Clone()
    {
        return Instantiate(this);
    }

    public abstract void Init();
    protected abstract void OnStart();
    protected abstract void OnStop();
    protected abstract State OnUpdate();
}

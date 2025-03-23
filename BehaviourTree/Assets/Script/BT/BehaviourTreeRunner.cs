using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTreeRunner : MonoBehaviour
{
    public BehaviourTree tree;
    Character info;

    private void Start()
    {
        info = GetComponent<Character>();
        tree = tree.Clone(info, transform);
    }

    private void Update()
    {
        if (info.OnUpdate)
        {
            tree.Update();
        }
    }
}
 
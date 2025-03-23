using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTreeRunner : MonoBehaviour
{
    public BehaviourTree tree;
    Information info;

    private void Start()
    {
        info = GetComponent<Information>();
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
 
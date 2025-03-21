using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectName { Attack, EffectName_End }

public class EffectManager : MonoBehaviour
{
    private static EffectManager _Instance;
    public static EffectManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType(typeof(EffectManager)) as EffectManager;
            }

            return _Instance;
        }
    }

    [SerializeField] List<GameObject> EffctObjects;
    Dictionary<int, Queue<EffectController>> EffctPool = new();

    [SerializeField] Transform CavansTransform;
    [SerializeField] GameObject DamageEffctObject;
    Queue<DamageValueEffect> damageValueEffectPool = new();


    //필요한 양 만큼 미리 생성
    private void Start()
    {
        for(int i = 0; i < (int)EffectName.EffectName_End; ++i)
        {
            Queue<EffectController> effectQueue = new();

            for (int j = 0; j < 5; ++j)
            {
                effectQueue.Enqueue(Instantiate(EffctObjects[i]).GetComponent<EffectController>());
            }

            EffctPool.Add(i, effectQueue);
        }

        for (int i = 0; i < 10; ++i)
        {
            DamageValueEffect DamafeEffect = Instantiate(DamageEffctObject, CavansTransform).GetComponent<DamageValueEffect>();
            damageValueEffectPool.Enqueue(DamafeEffect);
        }
    }

    public void TriggerDamageEffect(SKILLTYPE sKILLTYPE, Vector3 start_Point, Vector3 forward, int value)
    {
        DamageValueEffect emergeEffect = damageValueEffectPool.Dequeue();
        emergeEffect.StartEffect(sKILLTYPE, start_Point, forward, value);
        damageValueEffectPool.Enqueue(emergeEffect);
    }

    public void TriggerEffect(EffectName effectName, Vector3 start_Point, Vector3 forward)
    {
        EffectController emergeEffect = EffctPool[(int)effectName].Dequeue();
        emergeEffect.StartEffect(start_Point, forward);
        EffctPool[(int)effectName].Enqueue(emergeEffect);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    static DataManager _Instance;

    static public DataManager Instance()
    {
        if (_Instance == null)
        {
            _Instance = FindObjectOfType(typeof(DataManager)) as DataManager;
        }

        return _Instance;
    }

    [System.Serializable]
    public class SkillDataStruct
    {
        public SkillData[] skillDatas;
    }

    [SerializeField] HeroseStatData[] heroseStatDatas;
    [SerializeField] SkillDataStruct[] skillDatas;
    [SerializeField] HoldHeros[] holdHeros;

    public HoldHeros[] getHoldHeros
    {
        get { return holdHeros; }
    }

    public HeroseStatData GetHeroseStatData(int heroIndex)
    {
        return heroseStatDatas[heroIndex];
    }

    public SkillData[] GetSkillDatas(int heroIndex)
    {
        return skillDatas[heroIndex].skillDatas;
    }
}

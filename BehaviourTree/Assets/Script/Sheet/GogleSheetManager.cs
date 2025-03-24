using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GogleSheetManager : MonoBehaviour
{
    const string URL = "https://docs.google.com/spreadsheets/d/1j0AUaxpzewwu08Px0jSMOYvNoIgyTLpqiIzFbZGYVtY/export?format=tsv";
    [SerializeField] HeroStats HeroStats;

    IEnumerator Start()
    {
        //현재 홈페이지에 접근
        UnityWebRequest www = UnityWebRequest.Get(URL);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;
    }
}

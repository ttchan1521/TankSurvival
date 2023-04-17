using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardItem : MonoBehaviour
{
    [SerializeField] Text rank;
    [SerializeField] Text username;
    [SerializeField] Text score;

    public void UpdateView(string _rank, string _name, string _score)
    {
        rank.text = _rank;
        username.text = _name;
        score.text = _score;
    }
}

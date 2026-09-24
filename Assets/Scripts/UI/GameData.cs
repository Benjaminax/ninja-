using UnityEngine;
using Unity.Properties;
using System;

public class GameData : MonoBehaviour
{
    public static GameData instance;
    public static event Action OnScoreChanged;

    [SerializeField] private int m_Score;
    [SerializeField] private int m_CherryGemCount;

    [CreateProperty]
    public int Score
    {
        get => m_Score;
        set
        {
            m_Score = value;
            OnScoreChanged?.Invoke();
        }
    }

    public int CherryGemCount => m_CherryGemCount;

    public void CollectCherryGem()
    {
        m_CherryGemCount++;
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
}

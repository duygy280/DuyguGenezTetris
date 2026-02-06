using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TetrisManager : MonoBehaviour
{
    public int score { get; private set; }

    public bool gameOver { get; private set; }

    public UnityEvent OnScoreChanged;
    public UnityEvent OnGameOver;

    void Start()
    {
        SetGameOver(false);
    }

  
    public int CalculateScore(int clearedRows, Tetronimo pieceType)
    {
        int baseScore = 0;

        switch (clearedRows)
        {
            case 0: baseScore = 100; break;  
            case 1: baseScore = 300; break;
            case 2: baseScore = 500; break;
            case 3: baseScore = 800; break;
            default: baseScore = 100; break;
        }


        // F piece bonus
        if (pieceType == Tetronimo.F)
        {
            baseScore += 300; // extra point
        }

        return baseScore;
    }

    
    public void ChangeScoreWithPiece(int clearedRows, Tetronimo pieceType)
    {
        int scoreToAdd = CalculateScore(clearedRows, pieceType);
        ChangeScore(scoreToAdd);
    }

    
    public void ChangeScore(int amount)
    {
        score += amount;
        if (OnScoreChanged != null)
        {
            OnScoreChanged.Invoke();
        }
    }

    
    public void SetGameOver(bool _gameOver)
    {
        if (!_gameOver)
        {
            
            score = 0;
            ChangeScore(0);
        }

        gameOver = _gameOver;

        if (OnGameOver != null)
        {
            OnGameOver.Invoke();
        }
    }
}

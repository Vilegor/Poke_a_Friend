﻿using UnityEngine;

public class BaseCompetitorComponent : MonoBehaviour
{
    [Header("Level Settings")]
    public int maxLevel;

    [Header("Visual Progress Settings")]
    public GameObject objectContainer;
    public GameObject crownObject;
    public float maxYLength; // y position counts from 0
    
    [Header("Game State")]
    public int id;
    public int currentLevel;
    public bool isWinner;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // Called on Editor update
    public virtual void OnValidate()
    {
        ValidateLevelSetting();
        ValidateVisualProgressSetting();
        ForceSetLevel(currentLevel);
        SetWinner(isWinner);
    }

    protected void ValidateLevelSetting()
    {
        if (maxLevel < 1)
        {
            maxLevel = 1;
        }

        if (currentLevel < 0)
        {
            currentLevel = 0;
        }

        if (currentLevel > maxLevel)
        {
            currentLevel = maxLevel;
        }
    }

    protected void ValidateVisualProgressSetting()
    {
        if (maxYLength < 1.0f)
        {
            maxYLength = 1.0f;
        }
    }

    protected virtual void ForceSetLevel(int newLevel)
    {
        float newY = maxYLength * newLevel / maxLevel;
        Debug.Log($"Car #{id}. Level set to {newLevel} (Y = {newY})");

        objectContainer.transform.localPosition = new Vector3(0.0f, newY);
        currentLevel = newLevel;
    }

    public virtual void SetWinner(bool winner)
    {
        isWinner = winner;
        crownObject.SetActive(winner);
    }
}

﻿using TMPro;
using UnityEngine;

public class BaseCompetitorComponent : MonoBehaviour
{
    [Header("Level Settings")]
    public int maxLevel;

    [Header("Visual Progress Settings")]
    public GameObject objectContainer;
    public GameObject crown;
    public GameObject playerMarker;
    public float maxYLength; // y position counts from 0
    
    [Header("Health Points")]
    public TextMeshPro hpText;

    public int currentHp;
    
    [Header("Game State")]
    public int id;
    public int skinId;
    public int currentLevel;
    public bool isLeader;
    public bool isPlayer;

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
        ValidateSkinId();
        
        ForceSetLevel(currentLevel);
        UpdateSkin();
        SetWinner(isLeader);
        SetPlayerMarker(isPlayer);
        SetHealthPoints(currentHp);
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

    private void ValidateVisualProgressSetting()
    {
        if (maxYLength < 1.0f)
        {
            maxYLength = 1.0f;
        }
    }
    
    protected virtual void ValidateSkinId()
    {
        if (skinId < 1)
        {
            skinId = 1;
        }
    }

    protected virtual void UpdateSkin()
    {
        // override
    }

    private void ValidateHealthPoints()
    {
        if (currentHp < 0)
        {
            currentHp = 0;
        }
    }

    public void SetHealthPoints(int hp)
    {
        currentHp = hp;
        ValidateHealthPoints();

        hpText.text = currentHp.ToString();
    }

    protected virtual void ForceSetLevel(int newLevel)
    {
        float newY = maxYLength * newLevel / maxLevel;
        //Debug.Log($"Car #{id}. Level set to {newLevel} (Y = {newY})");

        objectContainer.transform.localPosition = new Vector3(0.0f, newY);
        currentLevel = newLevel;
    }

    public void SetWinner(bool winner)
    {
        isLeader = winner;
        crown.SetActive(winner);
    }

    public void SetPlayerMarker(bool player)
    {
        isPlayer = player;
        playerMarker.SetActive(player);
    }
}

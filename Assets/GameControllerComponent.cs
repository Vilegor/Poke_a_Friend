using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerComponent : MonoBehaviour
{
    [Header("Game Components")]
    public List<BaseCompetitorComponent> competitorObjects;
    
    [Header("Game UI Components")]
    public BaseGameButtonComponent boostButton;
    public KickButtonComponent kickButton;

    [Header("Game Setting")]
    public int startLevel;    // min level is always 0. 0 means death
    public int maxLevel;
    
    public float maxYLength; // y position counts from 0
    
    public int boostActionValue;
    public int boostActionCooldown;
    public int kickActionValue;
    public int kickActionCooldown;

    private void OnValidate()
    {
        boostButton.actionValue = boostActionValue;
        boostButton.cooldownTimeSeconds = boostActionCooldown;
        boostButton.OnValidate();
        
        kickButton.actionValue = kickActionValue;
        kickButton.cooldownTimeSeconds = kickActionCooldown;
        kickButton.OnValidate();

        if (startLevel < 1)
        {
            startLevel = 1;
        }
        if (maxLevel <= startLevel)
        {
            maxLevel = startLevel + 1;
        }

        int competitorsCount = competitorObjects.Count;
        int levelStep = maxLevel / (competitorsCount - 2);
        for (var i = 0; i < competitorsCount; i++)
        {
            var competitor = competitorObjects[i];
            
            competitor.id = i;
            competitor.maxLevel = maxLevel;
            competitor.maxYLength = maxYLength;
            
            competitor.isWinner = (i == competitorsCount - 2);
            
            if (i == competitorsCount - 1)
            {
                competitor.currentLevel = startLevel;
            }
            else
            {
                competitor.currentLevel = levelStep * i;
            }
            competitor.OnValidate();
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        boostButton.AddClickListener(OnBoostButtonClicked);
        kickButton.AddClickListener(OnKickButtonClicked);
    }

    private void OnBoostButtonClicked()
    {
        kickButton.RestartCooldown(boostActionCooldown);
    }
    
    private void OnKickButtonClicked()
    {
        boostButton.RestartCooldown(kickActionCooldown);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

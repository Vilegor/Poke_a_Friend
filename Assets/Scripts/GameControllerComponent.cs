using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buttons;

public class GameControllerComponent : MonoBehaviour
{
    [Header("Game Components")]
    public List<BaseCompetitorComponent> competitorObjects;
    
    [Header("Game UI Components")]
    public BoostButtonComponent boostButton;
    public KickButtonComponent kickButton;

    [Header("Game Setting")]
    public int startLevel;    // min level is always 0. 0 means death
    public int maxLevel;
    
    public float maxYLength; // y position counts from 0
    
    public int boostActionValue;
    public float boostActionCooldown;
    public float turboActionCooldown;
    public int kickActionValue;
    public float kickActionCooldown;

    private void OnValidate()
    {
        boostButton.actionValue = boostActionValue;
        boostButton.cooldownTimeSeconds = boostActionCooldown;
        boostButton.turboCooldownTimeSeconds = turboActionCooldown;
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
                competitor.isPlayer = true;
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
        float cooldown = boostButton.isTurbo ? turboActionCooldown : boostActionCooldown;
        boostButton.RestartCooldown(cooldown);
        kickButton.RestartCooldown(cooldown);
    }
    
    private void OnKickButtonClicked()
    {
        kickButton.RestartCooldown(kickActionCooldown);
        boostButton.RestartCooldown(kickActionCooldown);
    }

    private void StartGame()
    {
        foreach (var competitor in competitorObjects)
        {
            competitor.currentLevel = startLevel;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

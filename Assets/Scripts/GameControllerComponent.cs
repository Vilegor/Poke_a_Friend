﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameData;
using UnityEngine.UI;

public class GameControllerComponent : MonoBehaviour
{
    [Header("Game UI Components")]
    public GameUiController gameUiController;
    public GameObject startGameUiContainer;
    public GameObject endGameUiContainer;

    public Button startGameButton;
    public Button replayGameButton;
    public Text playerWonText;
    public Text gameTimeText;

    [Header("Competitor Setting")]
    public List<BaseCompetitorComponent> competitorObjects;    // views

    public int maxSkinId;

    [Header("Game Setting")]
    public int startLevel;    // min level is always 0. 0 means death
    public int maxLevel;
    
    public float maxYLength; // y position counts from 0

    public int startHealthPoints;
    
    [Header("- Bot Setting -")]
    public bool isBotGame;
    
    [Header("- Action Setting -")]
    public int boostActionValue;
    public float boostActionCooldown;
    public float turboActionCooldown;
    
    public int attackActionValue;    // TODO: separate values for HP and Level
    public float attackActionCooldown;

    [Header("- Action Modifications -")]
    public int eliminationBonusValue;    // TODO: maybe give bonus only for eliminating a leader "a King"
    public bool attackAlsoReducesLevel;
    
    // private game data
    private List<CompetitorModel> _competitorModels;
    private float _currentGameTime;
    private bool _isGameRunning;

    private CompetitorModel _myPlayerModel;

    private void OnValidate()
    {
        gameUiController.SetupActionsUi(boostActionValue, boostActionCooldown, turboActionCooldown, attackActionValue, attackActionCooldown);
        
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
            competitor.currentHp = startHealthPoints;
            
            competitor.isLeader = (i == competitorsCount - 2);
            competitor.isPlayer = (i == competitorsCount - 1);
            competitor.currentLevel = (i == competitorsCount - 1) ? startLevel : (i == competitorsCount - 2) ? maxLevel : levelStep * i;
            
            competitor.OnValidate();
        }
    }

    private void OnBoostButtonClicked()
    {
        if(!gameUiController.boostButton.interactable) return;

        gameUiController.DisableUi(boostActionCooldown);
        _myPlayerModel.RequestPlayerAction(ActionType.Boost);
    }

    private void OnTurboBoostButtonClicked()
    {
        if(!gameUiController.turboButton.interactable) return;
        
        gameUiController.DisableUi(turboActionCooldown);
        _myPlayerModel.RequestPlayerAction(ActionType.TurboBoost);
    }
    
    private void OnAttackLeaderButtonClicked()
    {
        if(!gameUiController.attackLeaderButton.interactable) return;
        
        gameUiController.DisableUi(attackActionCooldown);
        _myPlayerModel.RequestPlayerAction(ActionType.AttackLeader);
    }
    
    private void OnAttackLastButtonClicked()
    {
        if(!gameUiController.attackLastButton.interactable) return;
        
        gameUiController.DisableUi(attackActionCooldown);
        _myPlayerModel.RequestPlayerAction(ActionType.AttackLast);
    }

    private void OnStartGameClicked()
    {
        StartGame();
    }

    private void InitCompetitorModels()
    {
        _competitorModels = new List<CompetitorModel>();

        for (var i = 0; i < competitorObjects.Count; i++)
        {
            var model = new CompetitorModel(i, competitorObjects[i]);
            _competitorModels.Add(model);
        }
    }

    #region Game Life Cycle

    void Start()
    {
        InitCompetitorModels();
        
        gameUiController.boostButton.onClick.AddListener(OnBoostButtonClicked);
        gameUiController.turboButton.onClick.AddListener(OnTurboBoostButtonClicked);
        gameUiController.attackLeaderButton.onClick.AddListener(OnAttackLeaderButtonClicked);
        gameUiController.attackLastButton.onClick.AddListener(OnAttackLastButtonClicked);
        
        startGameButton.onClick.AddListener(OnStartGameClicked);
        replayGameButton.onClick.AddListener(OnStartGameClicked);

        _isGameRunning = false;
        endGameUiContainer.SetActive(false);
        startGameUiContainer.SetActive(true);
    }
    
    private void StartGame()
    {
        ReshuffleCompetitors(isBotGame, 1);
        
        foreach (var competitor in competitorObjects)
        {
            competitor.OnValidate();
        }
        
        gameUiController.SetActive(!isBotGame);
        startGameUiContainer.SetActive(false);
        endGameUiContainer.SetActive(false);

        _currentGameTime = 0;
        _isGameRunning = true;
    }

    private void ReshuffleCompetitors(bool isBotsOnly, int playerSkinId = 0)
    {
        _competitorModels.Sort((m1, m2) => m1.PlayerId - m2.PlayerId);
        
        var skinPool = Enumerable.Range(1, maxSkinId).OrderBy(x => Random.value).ToList();
        var playerIndex = isBotsOnly ? _competitorModels.Count : 0;    // Random.Range(0, _competitorModels.Count - 1)
        
        if (!isBotsOnly && playerSkinId != 0)
        {
            skinPool.Remove(playerSkinId);
            skinPool.Insert(playerIndex, playerSkinId);
        }

        for (var i = 0; i < _competitorModels.Count; i++)
        {
            var skinId = skinPool[i];
            _competitorModels[i].Reset(startLevel, startHealthPoints, skinId, i == playerIndex);
        }

        _myPlayerModel = isBotsOnly ? null : _competitorModels[playerIndex];
    }

    void Update()
    {
        if (!_isGameRunning) return;

        // update game time
        var elapsed = Time.deltaTime;
        _currentGameTime += elapsed;

        // prepare list of only active players
        var activePlayers = GetActivePlayers();
        
        // update My Player UI
        UpdateGameUi(activePlayers);
        
        // collect action requests from players
        var actionRequests = CollectActionRequests(activePlayers, elapsed);

        // perform actions in order
        ProcessRequestActions(actionRequests, activePlayers);
        
        // Update players leaderboard and define new Leader
        UpdateLeader();
        
        //Debug.Log($"Updated: Leader #{_competitorModels[0].PlayerId}, lvl {_competitorModels[0].CurrentLevel}");

        foreach (var view in competitorObjects)
        {
            view.OnValidate();
        }

        if (_competitorModels[0].CurrentLevel < maxLevel) return;
        
        _isGameRunning = false;
        OnGameEnded(_competitorModels[0], _currentGameTime);
    }

    private List<CompetitorModel> GetActivePlayers()
    {
        var activeCompetitors = new List<CompetitorModel>();
        foreach (var comp in _competitorModels)
        {
            if (!comp.IsEliminated)
            {
                activeCompetitors.Add(comp);
            }
        }

        return activeCompetitors;
    }
    private void UpdateGameUi(List<CompetitorModel> activePlayers)
    {
        if (_myPlayerModel == null) return;
        if (_myPlayerModel.IsEliminated) gameUiController.DisableUi();
        
        var leaderId = _myPlayerModel.IsLeader ? -1 : activePlayers[0].PlayerId;
        
        var last = activePlayers[activePlayers.Count - 1];
        var lastId = _myPlayerModel == last ? -1 : last.PlayerId;
        
        gameUiController.UpdateUi(_myPlayerModel.IsLeader, leaderId, lastId);
    }

    private List<ActionRequest> CollectActionRequests(List<CompetitorModel> activePlayers, float elapsedTime)
    {
        var actionRequests = new List<ActionRequest>();

        foreach (var player in activePlayers)
        {
            var availableActions = new List<ActionType>();
            if (player.PendingCooldown < elapsedTime)
            {
                availableActions.Add(player.IsLeader ? ActionType.TurboBoost : ActionType.Boost);
                if (!player.IsLeader)
                {
                    availableActions.Add(ActionType.AttackLeader);
                }
                if (player != activePlayers[activePlayers.Count - 1])
                {
                    availableActions.Add(ActionType.AttackLast);
                }
            }

            var request = player.Update(elapsedTime, availableActions, activePlayers);
            if (request.Type != ActionType.None)
            {
                actionRequests.Add(request);
            }
        }

        return actionRequests;
    }

    private void ProcessRequestActions(List<ActionRequest> actionRequests, List<CompetitorModel> activePlayers)
    {
        // sort all action request by Time
        actionRequests.Sort((r1, r2) => r2.CompareTo(r1));
        
        foreach (var request in actionRequests)
        {
            Debug.Log($"Bot #{request.PlayerId} Action = {request.Type}");
            switch (request.Type)
            {
                case ActionType.Boost:
                    GetCompetiroById(request.PlayerId).PerformBoostAction(boostActionValue, boostActionCooldown);
                    break;
                case ActionType.TurboBoost:
                    GetCompetiroById(request.PlayerId).PerformBoostAction(boostActionValue, turboActionCooldown);
                    break;
                case ActionType.AttackLeader:
                    var target1 = activePlayers[0];
                    
                    target1.ApplyAttackAction(attackActionValue, attackAlsoReducesLevel);
                    GetCompetiroById(request.PlayerId).PerformAttackAction(attackActionCooldown);
                    
                    if (target1.IsEliminated && eliminationBonusValue > 0)
                    {
                        GetCompetiroById(request.PlayerId).PerformBoostAction(eliminationBonusValue, attackActionCooldown);
                    }
                    break;
                case ActionType.AttackLast:
                    var target8 = activePlayers[activePlayers.Count - 1];
                    
                    target8.ApplyAttackAction(attackActionValue, attackAlsoReducesLevel);
                    GetCompetiroById(request.PlayerId).PerformAttackAction(attackActionCooldown);
                    
                    if (target8.IsEliminated && eliminationBonusValue > 0)
                    {
                        GetCompetiroById(request.PlayerId).PerformBoostAction(eliminationBonusValue, attackActionCooldown);
                    }
                    break;
                case ActionType.None:
                    Debug.LogError("NONE Action!");    // ERROR!
                    break;
            }
        }
    }

    private void UpdateLeader()
    {
        // update leaderbord. [0] is a leader
        
        _competitorModels.Sort((c1, c2) => c2.CompareTo(c1));
        for (var i = 0; i < _competitorModels.Count; i++)
        {
            _competitorModels[i].IsLeader = (i == 0);
        }
    }

    private CompetitorModel GetCompetiroById(int playerId)
    {
        foreach (var model in _competitorModels)
        {
            if (model.PlayerId == playerId) return model;
        }

        return null;
    }
    
    private void OnGameEnded(CompetitorModel winnerModel, float gameTime)
    {
        if (winnerModel.IsPlayer)
        {
            playerWonText.text = "YOU WON!";
        }
        else
        {
            playerWonText.text = $"Player #{winnerModel.PlayerId + 1} WON!";
        }

        gameTimeText.text = $"in {gameTime} seconds";
        endGameUiContainer.SetActive(true);
    }

    #endregion
}

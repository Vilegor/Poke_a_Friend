using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Buttons;
using GameData;
using UnityEngine.UI;

public class GameControllerComponent : MonoBehaviour
{
    [Header("Game UI Components")]
    public GameObject gameUiContainer;
    public GameObject startGameUiContainer;
    public GameObject endGameUiContainer;

    public Button startGameButton;
    public Button replayGameButton;
    public Text playerWonText;
    public Text gameTimeText;
    
    public BoostButtonComponent boostButton;
    public KickButtonComponent kickButton;

    [Header("Competitor Setting")]
    public List<BaseCompetitorComponent> competitorObjects;    // views

    public int maxSkinId;
    
    [Header("Game Setting")]
    public int startLevel;    // min level is always 0. 0 means death
    public int maxLevel;
    
    public float maxYLength; // y position counts from 0
    
    public int boostActionValue;
    public float boostActionCooldown;
    public float turboActionCooldown;
    public int kickActionValue;
    public float kickActionCooldown;
    
    // private game data
    private List<CompetitorModel> _competitorModels;
    private float _currentGameTime;
    private bool _isGameRunning;

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
            
            competitor.isLeader = (i == competitorsCount - 2);
            
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
        boostButton.AddClickListener(OnBoostButtonClicked);
        kickButton.AddClickListener(OnKickButtonClicked);
        
        startGameButton.onClick.AddListener(OnStartGameClicked);
        replayGameButton.onClick.AddListener(OnStartGameClicked);

        _isGameRunning = false;
        endGameUiContainer.SetActive(false);
        startGameUiContainer.SetActive(true);
    }
    
    private void StartGame()
    {
        ReshuffleCompetitors(true);
        
        foreach (var competitor in competitorObjects)
        {
            competitor.OnValidate();
        }
        
        startGameUiContainer.SetActive(false);
        endGameUiContainer.SetActive(false);

        _currentGameTime = 0;
        _isGameRunning = true;
    }

    private void ReshuffleCompetitors(bool botGame, int playerSkinId = 0)
    {
        var skinPool = Enumerable.Range(1, maxSkinId).OrderBy(x => Random.value).ToList();
        var playerIndex = botGame ? _competitorModels.Count : Random.Range(0, _competitorModels.Count - 1);
        
        if (playerSkinId != 0)
        {
            skinPool.Remove(playerSkinId);
            skinPool.Insert(playerIndex, playerSkinId);
        }

        for (var i = 0; i < _competitorModels.Count; i++)
        {
            var skinId = skinPool[i];
            _competitorModels[i].Reset(startLevel, skinId, i == playerIndex);
        }
    }

    void Update()
    {
        if (!_isGameRunning) return;

        // update game time
        var elapsed = Time.deltaTime;
        _currentGameTime += elapsed;

        // collect action requests from players
        var actionRequests = new List<ActionRequest>();
        foreach (var competitor in _competitorModels)
        {
            var availableActions = new List<ActionType>();
            if (competitor.PendingCooldown < elapsed)
            {
                availableActions.Add(competitor.IsLeader ? ActionType.TurboBoost : ActionType.Boost);
                availableActions.Add(ActionType.Attack);
            }
            var request = competitor.Update(elapsed, availableActions);
            if (request.Type != ActionType.None)
            {
                actionRequests.Add(request);
            }
        }
        
        // sort all action request by Time
        actionRequests.Sort((r1, r2) => r2.CompareTo(r1));

        foreach (var request in actionRequests)
        {
            Debug.Log($"ActionType = {request.Type}, id = {request.PlayerId}");
            switch (request.Type)
            {
                case ActionType.Attack:
                    _competitorModels[0].ApplyKickDownAction(kickActionValue);
                    GetCompetiroById(request.PlayerId).PerformKickDownAction(kickActionCooldown);
                    break;
                case ActionType.Boost:
                    GetCompetiroById(request.PlayerId).PerformBoostAction(boostActionValue, boostActionCooldown);
                    break;
                case ActionType.TurboBoost:
                    GetCompetiroById(request.PlayerId).PerformBoostAction(boostActionValue, turboActionCooldown);
                    break;
                case ActionType.None:
                    Debug.LogError("NONE Action!");    // ERROR!
                    break;
            }
            UpdateLeader();
        }

        foreach (var view in competitorObjects)
        {
            view.OnValidate();
        }

        if (_competitorModels[0].CurrentLevel < maxLevel) return;
        
        _isGameRunning = false;
        OnGameEnded(_competitorModels[0], _currentGameTime);
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
            playerWonText.text = $"Player #{winnerModel.PlayerId} WON!";
        }

        gameTimeText.text = $"in {gameTime} seconds";
        endGameUiContainer.SetActive(true);
    }

    #endregion
}

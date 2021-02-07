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
    public bool isBotGame;
    public int startLevel;    // min level is always 0. 0 means death
    public int maxLevel;
    
    public float maxYLength; // y position counts from 0
    
    public int boostActionValue;
    public float boostActionCooldown;
    public float turboActionCooldown;
    public int attackActionValue;
    public float attackActionCooldown;
    public int eliminationBonusValue;
    
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
        
        kickButton.actionValue = attackActionValue;
        kickButton.cooldownTimeSeconds = attackActionCooldown;
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
            competitor.isPlayer = (i == competitorsCount - 1);
            competitor.currentLevel = (i == competitorsCount - 1) ? startLevel : (i == competitorsCount - 2) ? maxLevel : levelStep * i;
            
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
        kickButton.RestartCooldown(attackActionCooldown);
        boostButton.RestartCooldown(attackActionCooldown);
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
        ReshuffleCompetitors(isBotGame);
        
        foreach (var competitor in competitorObjects)
        {
            competitor.OnValidate();
        }
        
        gameUiContainer.SetActive(!isBotGame);
        startGameUiContainer.SetActive(false);
        endGameUiContainer.SetActive(false);

        _currentGameTime = 0;
        _isGameRunning = true;
    }

    private void ReshuffleCompetitors(bool isBotsOnly, int playerSkinId = 0)
    {
        var skinPool = Enumerable.Range(1, maxSkinId).OrderBy(x => Random.value).ToList();
        var playerIndex = isBotsOnly ? _competitorModels.Count : Random.Range(0, _competitorModels.Count - 1);
        
        if (!isBotsOnly && playerSkinId != 0)
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

        var activeCompetitors = new List<CompetitorModel>();
        foreach (var comp in _competitorModels)
        {
            if (!comp.IsEliminated)
            {
                activeCompetitors.Add(comp);
            }
        }
        
        foreach (var competitor in activeCompetitors)
        {
            var availableActions = new List<ActionType>();
            if (competitor.PendingCooldown < elapsed)
            {
                availableActions.Add(competitor.IsLeader ? ActionType.TurboBoost : ActionType.Boost);
                if (!competitor.IsLeader)
                {
                    availableActions.Add(ActionType.AttackLeader);
                }
                if (competitor != activeCompetitors[activeCompetitors.Count - 1])
                {
                    availableActions.Add(ActionType.AttackLast);
                }
            }

            var request = competitor.Update(elapsed, availableActions, activeCompetitors);
            if (request.Type != ActionType.None)
            {
                actionRequests.Add(request);
            }
        }
        
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
                    var target1 = _competitorModels[0];

                    if (target1.IsEliminated) break;
                    
                    target1.ApplyAttackAction(attackActionValue);
                    GetCompetiroById(request.PlayerId).PerformAttackAction(attackActionCooldown);
                    
                    if (target1.IsEliminated)
                    {
                        GetCompetiroById(request.PlayerId).PerformBoostAction(eliminationBonusValue, attackActionCooldown);
                    }
                    break;
                case ActionType.AttackLast:
                    // TODO: what if he's the LAST and attacks himself?
                    var target8 = _competitorModels[_competitorModels.Count - 1];
                    if (target8.IsEliminated) break;
                    
                    target8.ApplyAttackAction(attackActionValue);
                    GetCompetiroById(request.PlayerId).PerformAttackAction(attackActionCooldown);
                    
                    if (target8.IsEliminated)
                    {
                        GetCompetiroById(request.PlayerId).PerformBoostAction(eliminationBonusValue, attackActionCooldown);
                    }
                    break;
                case ActionType.None:
                    Debug.LogError("NONE Action!");    // ERROR!
                    break;
            }
        }
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

    private void UpdateLeader()
    {
        // update leaderbord. [0] is a leader
        
        _competitorModels.Sort((c1, c2) => c2.CompareTo(c1));
        for (var i = 0; i < _competitorModels.Count; i++)
        {
            _competitorModels[i].IsLeader = (i == 0);
            // TODO: Process losers with level = 0!! kick them off!
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

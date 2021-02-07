using System.Collections.Generic;
using UnityEngine;

namespace GameData
{
    public class CompetitorModel
    {
        private int _id;
        private BaseCompetitorComponent _view;

        public int PlayerId => _view.id;
        public int CurrentLevel => _view.currentLevel;
        public int SkinId => _view.skinId;
        public bool IsPlayer => _view.isPlayer;

        public float PendingCooldown => _pendingCooldown;

        public bool IsLeader
        {
            get => _view.isLeader;
            set => _view.isLeader = value;
        }

        public bool IsEliminated { get; private set; }

        // current game state
        private float _pendingCooldown = 0;
        
        // bot settings
        private float _botActionDelay = 0;
        private BotStrategyType _botStrategy;
        
        // player settings
        private ActionType _pendingActionType;

        public CompetitorModel(int id, BaseCompetitorComponent view)
        {
            _id = id;
            _view = view;
        }

        public int CompareTo(CompetitorModel c)
        {
            if (CurrentLevel == c.CurrentLevel)
            {
                if (PendingCooldown < c.PendingCooldown)
                {
                    return 1;
                }
                if (PendingCooldown > c.PendingCooldown)
                {
                    return -1;
                }

                return 0;
            }

            return CurrentLevel - c.CurrentLevel;
        }

        public void Reset(int startLevel, int skinId, bool isPlayer = false)
        {
            _view.currentLevel = startLevel;
            _view.skinId = skinId;
            _view.isPlayer = isPlayer;
            _view.isLeader = false;
            IsEliminated = false;
            _botActionDelay = Random.Range(0.0f, 0.1f);
            _botStrategy = PlayerId == 0 ? BotStrategyType.Aggressive : BotStrategyType.Dumbster; //(BotStrategyType)Random.Range(0, 3);    // from Dumb to Aggr
            _pendingActionType = ActionType.None;
            _pendingCooldown = 0.0f;

            if (!isPlayer)
            {
                Debug.Log($"Bot Reset #{PlayerId}: d={_botActionDelay}, s={_botStrategy}");
            }
        }

        public void PerformBoostAction(int value, float cooldown)
        {
            _view.currentLevel += value;
            _pendingCooldown = cooldown;
        }

        public void PerformAttackAction(float cooldown)
        {
            _pendingCooldown = cooldown;
        }

        public void ApplyAttackAction(int value)
        {
            _view.currentLevel -= value;

            IsEliminated = _view.currentLevel <= 0;
        }

        // UPDATE cycle

        public ActionRequest Update(float timeElapsed, List<ActionType> availableActions, List<CompetitorModel> allCompetitors)
        {
            return IsPlayer ? PlayerUpdate(timeElapsed, availableActions) : BotUpdate(timeElapsed, availableActions, allCompetitors);
        }
        
        private ActionRequest BotUpdate(float timeElapsed, List<ActionType> availableActions, List<CompetitorModel> allCompetitors)
        {
            if (_pendingCooldown > _botActionDelay * -1.0f)
            {
                _pendingCooldown -= timeElapsed;
            }

            var action = ActionType.None;
            if (_botActionDelay + _pendingCooldown <= 0)
            {
                action = CalculateBotAction(availableActions, allCompetitors);
            }
            
            return new ActionRequest(PlayerId, action, _pendingCooldown);
        }

        private ActionType CalculateBotAction(List<ActionType> availableActions, List<CompetitorModel> allCompetitors)
        {
            // always TurboBoos is Leader
            if (availableActions.Contains(ActionType.TurboBoost)) return ActionType.TurboBoost;
            
            ActionType finalAction = ActionType.None;
            
            // non-Leader strategy section !
            var leader = allCompetitors[0];
            var last = allCompetitors[allCompetitors.Count - 1];
            
            switch (_botStrategy)
            {
                case BotStrategyType.Dumbster:
                    finalAction = ActionType.Boost;
                    break;
                case BotStrategyType.Aggressive:
                    if (this == last)
                    {
                        finalAction = leader.CurrentLevel <= allCompetitors.Count * 0.5f
                            ? ActionType.AttackLeader
                            : ActionType.Boost;
                    }
                    else if (last.CurrentLevel <= allCompetitors.Count * 0.4f)    // this guy wants to be sure that if almost half of the players will shoot the Last he'd be gone
                    {
                        finalAction = ActionType.AttackLast;
                    }
                    else if (leader.CurrentLevel >= _view.maxLevel * 0.7f || leader.CurrentLevel - CurrentLevel >= _view.maxLevel * 0.2f)
                    {
                        finalAction = ActionType.AttackLeader;
                    }
                    else
                    {
                        finalAction = ActionType.Boost;
                    }
                    
                    break;
                case BotStrategyType.Smartass:
                    var aggressiveLevelThreshold = _view.maxLevel * 0.8f;
                    var minLeaderDiffThreshold = _view.maxLevel * 0.2f;
                    var maxLeaderDiffThreshold = _view.maxLevel * 0.3f;
                    var attackLastThreshold = allCompetitors.Count * 0.3f;
                    
                    if (this == allCompetitors[1])    // he's #2
                    {
                        if (leader.CurrentLevel >= aggressiveLevelThreshold)
                        {
                            if (last.CurrentLevel <= attackLastThreshold)
                            {
                                finalAction = ActionType.AttackLast;
                            }
                            else if (leader.CurrentLevel - CurrentLevel <= minLeaderDiffThreshold)
                            {
                                finalAction = ActionType.AttackLeader;
                            }
                            else
                            {
                                finalAction = ActionType.Boost;
                            }
                        }
                        else
                        {
                            finalAction = leader.CurrentLevel - CurrentLevel > maxLeaderDiffThreshold
                                ? ActionType.AttackLeader
                                : ActionType.Boost;
                        }
                    }
                    else
                    {
                        if (last.CurrentLevel <= attackLastThreshold)
                        {
                            finalAction = ActionType.AttackLast;
                        }
                        else if (leader.CurrentLevel - CurrentLevel > maxLeaderDiffThreshold)
                        {
                            finalAction = ActionType.AttackLeader;
                        }
                        else
                        {
                            finalAction = ActionType.Boost;
                        }
                    }
                    
                    break;
            }

            if (!availableActions.Contains(finalAction))
            {
                Debug.LogWarning($"Bot #{PlayerId}: Action calculation FAILED!");
                finalAction = availableActions[0];
            }

            return finalAction;
        }

        private ActionRequest PlayerUpdate(float timeElapsed, List<ActionType> availableActions)
        {
            if (_pendingCooldown > 0)
            {
                _pendingCooldown -= timeElapsed;
            }

            if (_pendingCooldown > 0 || !availableActions.Contains(_pendingActionType)) return new ActionRequest(PlayerId, ActionType.None);
            
            var action = _pendingActionType;
            _pendingActionType = ActionType.None;
            return new ActionRequest(PlayerId, action, _pendingCooldown);
        }
    }
}
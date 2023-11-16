﻿using NeoNetsphere.Network;
using NeoNetsphere.Network.Message.GameRule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoNetsphere.Game.GameRules
{
    internal class ArcadeGameRule : GameRuleBase
    {

        public override bool CountMatch => false;

        private Dictionary<ulong, Player> _loadingOk = new Dictionary<ulong, Player>();

        public override Briefing Briefing { get; }

        public override GameRule GameRule => GameRule.Arcade;

        public ArcadeGameRule(Room room)
            : base(room)
        {
            Briefing = new Briefing(this);

            StateMachine.Configure(GameRuleState.Waiting)
                .PermitIf(GameRuleStateTrigger.StartGame, GameRuleState.FullGame, CanStart);

            StateMachine.Configure(GameRuleState.FullGame)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.StartResult, GameRuleState.EnteringResult);

            StateMachine.Configure(GameRuleState.EnteringResult)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.StartResult, GameRuleState.Result);

            StateMachine.Configure(GameRuleState.Result)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.EndGame, GameRuleState.Waiting);
        }

        public override void Initialize()
        {
            Room.TeamManager.Add(
                Team.Alpha,
                (uint)Room.Options.PlayerLimit,
                (uint)Room.Options.Spectator);
            base.Initialize();
        }

        public override void Cleanup()
        {
            Room.TeamManager.Remove(Team.Alpha);
            base.Cleanup();
        }

        public override void Update(AccurateDelta delta)
        {
            base.Update(delta);
            var teamMgr = Room.TeamManager;

            if (StateMachine.IsInState(GameRuleState.Playing) &&
                !StateMachine.IsInState(GameRuleState.EnteringResult) &&
                !StateMachine.IsInState(GameRuleState.Result) &&
                RoundTime >= TimeSpan.FromSeconds(5)) // Let the round run for at least 5 seconds - Fixes StartResult trigger on game start(race condition)
            {
                if (StateMachine.IsInState(GameRuleState.FullGame))
                {
                    // Still have enough players?
                    if (teamMgr.PlayersPlaying.Any())
                        StateMachine.Fire(GameRuleStateTrigger.StartResult);

                    // Did we reach round limit?
                    if (RoundTime >= Room.Options.TimeLimit)
                        StateMachine.Fire(GameRuleStateTrigger.StartResult);
                }
            }
        }

        public override PlayerRecord GetPlayerRecord(Player plr)
        {
            return new ArcadePlayerRecord(plr);
        }

        public void OnLoadingOk(Player plr)
        {
            _loadingOk.Add(plr.Account.Id, plr);
            Room.Broadcast(new ArcadeSucceedLoadingAckMessage { AccountId = plr.Account.Id });

            if (_loadingOk.Count == Room.Players.Count)
                Room.Broadcast(new ArcadeSucceedLoadingAckMessage());
        }

        private bool CanStart()
        {
            return !Room.TeamManager.Players.Any(p => p.RoomInfo.IsReady == false && p != Room.Master);
        }
    }

    internal class ArcadePlayerRecord : PlayerRecord
    {
        public override uint TotalScore => 0;

        public ArcadePlayerRecord(Player plr)
            : base(plr)
        { }
    }
}

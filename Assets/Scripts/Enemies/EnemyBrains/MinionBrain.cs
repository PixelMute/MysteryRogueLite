using NesScripts.Controls.PathFind;
using System.Collections.Generic;


class MinionBrain : BasicEnemyAI
{
    public override void ActionPhase()
    {
        //Walk towards player and punch player
        engagedTarget = BattleManager.player;
        DecideCombatTurn();
    }

    public override void EndOfTurn()
    {
        return;
    }

    public override void StartOfTurn()
    {
        return;
    }

    public override void SummonForHelp(List<Point> pathToFollow)
    {
        //Don't respond to calls for help
        return;
    }
}


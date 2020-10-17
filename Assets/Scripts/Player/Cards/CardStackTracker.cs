// Tracks the results of a card stack.
// Used by cards to determine what happened as a result of them being played.
public class CardStackTracker
{
    private bool active; // Are we currently tracking stuff?
    private int damageDealt = 0; // How much damage has been dealt over this tracking session
    private Card currentlyResolvingCard; // If needed, we can make this an actual stack.
    private bool usedMomentum = false;

    public void AddCardToTracker(Card input)
    {
        currentlyResolvingCard = input;
        active = true;
    }

    // Resets how much we've tracked over the course of this session
    public void ResetTracker()
    {
        if (!active)
            return;

        active = false;
        damageDealt = 0;
        currentlyResolvingCard = null;
        usedMomentum = false;
    }

    // Have we dealt at least this much?
    public bool QueryDamageDealt(int input)
    {
        return (damageDealt >= input);
    }

    public void AddDamageDealt(int input)
    {
        if (active)
            damageDealt += input;
    }

    public bool IsActive()
    {
        return active;
    }

    public int GetMomentumBonus()
    {
        usedMomentum = true;
        return BattleManager.player.GetMomentumBonus();
    }

    public float GetInsightBonus()
    {
        return BattleManager.player.GetInsightBonus();
    }

    public void SetUsedMomentum(bool mo)
    {
        usedMomentum = mo;
    }

    public bool QueryUsedMomentum()
    {
        return usedMomentum;
    }
}

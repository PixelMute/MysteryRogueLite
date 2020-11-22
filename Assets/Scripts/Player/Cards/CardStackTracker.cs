// Tracks the results of a card stack.
// Used by cards to determine what happened as a result of them being played.
public class CardStackTracker
{
    private bool active; // Are we currently tracking stuff?
    private int damageDealt = 0; // How much damage has been dealt over this tracking session
    private CardInterface currentlyResolvingCard; // If needed, we can make this an actual stack.
    private int usedMomentum = 0; // the amount of momentium this card stack has taken.

    public CardInterface CurrentlyResolvingCard { get => currentlyResolvingCard; private set => currentlyResolvingCard = value; }

    public void AddCardToTracker(CardInterface input)
    {
        CurrentlyResolvingCard = input;
        active = true;
    }

    // Resets how much we've tracked over the course of this session
    public void ResetTracker()
    {
        if (!active)
            return;

        active = false;
        damageDealt = 0;
        CurrentlyResolvingCard = null;
        usedMomentum = 0;
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
        int momun = BattleManager.player.GetMomentumBonus();
        usedMomentum = UnityEngine.Mathf.Max(usedMomentum, momun);
        return momun;
    }

    public float GetInsightBonus()
    {
        return BattleManager.player.GetInsightBonus();
    }

    public void SetUsedMomentum(int mo)
    {
        usedMomentum = mo;
    }

    public int QueryUsedMomentum()
    {
        return usedMomentum;
    }

    public CardInterface GetCurrentlyResolvingCard()
    {
        return CurrentlyResolvingCard;
    }
}

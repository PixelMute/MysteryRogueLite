// This is a simple data class that stores the StatusEffectIconInterface alongside
// whatever data is associated with this instance of a status effect.
public class StatusEffectDataHolder
{
    public StatusEffectIconInterface iconInterface;
    private int effectValue;

    public int EffectValue 
    { 
        get => effectValue;
        set { effectValue = value; iconInterface.SetNumber(effectValue); }
    }
}

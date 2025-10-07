using System.Collections.Generic;

public interface IUsable
{
    List<UsableItemEffect> UsableItemEffects { get; }
    public void Use();
}
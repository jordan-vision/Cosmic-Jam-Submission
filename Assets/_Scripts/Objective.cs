using System;

public class Objective
{
    Predicate<PlayerInfo> condition;
    string description;
    int id;
    Action<PlayerInfo> whenScored;

    public Predicate<PlayerInfo> Condition => condition;
    public string Description => description;
    public int Id => id;
    public Action<PlayerInfo> WhenScored => whenScored;
    
    public Objective (string description, Predicate<PlayerInfo> condition, int id, Action<PlayerInfo> whenScored = null)
    {
        this.description = description;
        this.condition = condition;
        this.id = id;

        if (whenScored == null)
        {
            whenScored = (_) => { };
        }
        else
        {
            this.whenScored = whenScored;
        }
    }
}

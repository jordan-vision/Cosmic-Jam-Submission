using System;

public class Objective
{
    Predicate<PlayerInfo> condition;
    string description;
    int id;
    bool scored = false;

    public Predicate<PlayerInfo> Condition => condition;
    public string Description => description;
    public int Id => id;
    public bool Scored { get { return scored; } set { scored = value; } }
    
    public Objective (string description, Predicate<PlayerInfo> condition, int id)
    {
        this.description = description;
        this.condition = condition;
        this.id = id;
    }
}

[System.Serializable]
public class GoalMessage : Message
{
    public int points;
    public GoalMessage(int points)
    {
        this.type = "goal";
        this.points = points;
    }
}
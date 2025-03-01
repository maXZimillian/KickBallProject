[System.Serializable]
public class KickLateMessage : Message
{
    public KickLateMessage()
    {
        this.type = "kick_late";
    }
}
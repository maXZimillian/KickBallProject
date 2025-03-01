public class DisconnectMessage : Message
{
    public string playerID;
    public string content;
    public DisconnectMessage(string room, string playerID)
    {
        this.type = "disconnect";
        this.content = room;
        this.playerID = playerID;
    }
}

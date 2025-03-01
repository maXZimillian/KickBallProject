using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KickStartMessage : Message
{
    public Vector3[] swipeCoords;
    public string strikeType;
    public KickStartMessage(Vector3[] swipeCoords, StrikeType strikeType)
    {
        this.type = "kick_start";
        this.swipeCoords = swipeCoords;
        this.strikeType = strikeType.ToString();
    }

}

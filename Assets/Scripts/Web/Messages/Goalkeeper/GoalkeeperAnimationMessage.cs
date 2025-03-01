using UnityEngine;

[System.Serializable]
public class GoalkeeperAnimationMessage : Message
{
    public string animationType;
    public GoalkeeperAnimationMessage(string animationType)
    {
        this.type = "gk_animation";
        this.animationType = animationType;
    }
}
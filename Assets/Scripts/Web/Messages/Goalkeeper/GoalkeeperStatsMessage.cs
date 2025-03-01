using UnityEngine;

[System.Serializable]
public class GoalkeeperStatsMessage : Message
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 colliderScale;
    public string animation;

    public GoalkeeperStatsMessage(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 colliderScale, string animation)
    {
        this.type = "gk_stats";
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
        this.colliderScale = colliderScale;
        this.animation = animation;
    }
}
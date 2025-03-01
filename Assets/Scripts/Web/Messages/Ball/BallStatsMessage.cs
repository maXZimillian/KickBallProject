using UnityEngine;

[System.Serializable]
public class BallStatsMessage : Message
{
    public float[] position;
    public float[] rotation;
    public float[] velocity;
    public BallStatsMessage(Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        this.type = "ball_stats";
        this.position = new float[]{position.x, position.y, position.z};
        this.rotation = new float[]{rotation.w, rotation.x, rotation.y, rotation.z};
        this.velocity = new float[]{velocity.x, velocity.y, velocity.z};
    }
}
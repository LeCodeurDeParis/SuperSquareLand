using System;

[Serializable]
public class HeroDashSettings
{
    public float Speed = 40f;
    public float Duration = 0.1f;
    [System.NonSerialized]
    public float timer = 0f;
    [System.NonSerialized]
    public bool isDashing = false;

    
}

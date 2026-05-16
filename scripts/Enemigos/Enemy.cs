using Godot;
using System;

namespace Faeterna.Scripts.Enemigos
{
    public partial class Enemy : CharacterBody2D
    {
        // Called when the node enters the scene tree for the first time.
        [Export] public int health;
        [Export] public float dashSpeed;
        [Export] public float jumpVelocity;
        [Export] public AnimatedSprite2D animatedSprite;
        [Export] public float dashDuration;
        [Export]  public float knockbackDuration;
        [Export] public bool flipSprite; 
        [Export] public AudioStreamPlayer2D audioPlayer;

        [ExportGroup("Collisions and Areas")]
        [Export] public CollisionShape2D detectionArea;
        [Export] public RayCast2D groundCheck;
        [Export] public Area2D hurtBox;
        [Export] public Area2D attackHitBox;

        public int dashDirection = 1;
        public float knockbackTimer = 0f;
        public ShaderMaterial shaderMaterial;
        
        public Node2D target = null;
        public static float Gravity => ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
        public override void _Ready()
        {
            if (animatedSprite == null)
                animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        }
        public async void hitShader(ShaderMaterial shader)
        {
            shader.SetShaderParameter("white_amount", 1.0f);

            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

            shader.SetShaderParameter("white_amount", 0.0f);
        }

        public void playAudio(AudioStream audio)
        {
            audioPlayer.Stream = audio;
            audioPlayer.Play();
        }

        public void SetAnimation(string animationName)
        {
            animatedSprite?.Play(animationName);
        }
    }
}

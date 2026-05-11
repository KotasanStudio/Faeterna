using Godot;
using System;

namespace Faeterna.Scripts.Enemigos.Enemy
{	
public partial class Enemy : CharacterBody2D
{
	// Called when the node enters the scene tree for the first time.
    [Export] public AnimatedSprite2D _animatedSprite;


	public override void _Ready()
        {
			if (_animatedSprite == null)
				_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            
        }

	        public async void hitShader(ShaderMaterial _shaderMaterial)
        {
            _shaderMaterial.SetShaderParameter("white_amount", 1.0f);

            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

            _shaderMaterial.SetShaderParameter("white_amount", 0.0f);
        }
		
		public void SetAnimation(string animationName)
        {
            _animatedSprite?.Play(animationName);
        }
}
}

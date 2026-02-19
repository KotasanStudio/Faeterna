using Godot;
using System;

namespace Faeterna.scripts.Player
{
	public partial class Lira : CharacterBody2D
	{
		/// <summary>Velocidad horizontal configurada del jugador (px/s).</summary>
		public const float Speed = 400.0f;

		/// <summary>Velocidad vertical aplicada al iniciar un salto (negativa = hacia arriba).</summary>
		public const float JumpVelocity = -500.0f;

		public bool DoubleJumpAvailable = true;

		public bool DashAvailable = true;

		public bool CoyoteAvailable = true;

		public AnimatedSprite2D animatedSprite;
        public override void _Ready()
		{
			animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		}

		/// <summary>Actualización de física del jugador. Mantener ligera para lógica central.</summary>
		/// <param name="delta">Tiempo en segundos desde el último paso de física.</param>
		// public override void _PhysicsProcess(double delta)
		// {
		// 	if (Velocity.X < 0f)
		// 		animatedSprite.FlipH = true;
		// 	else if (Velocity.X > 0f)
		// 		animatedSprite.FlipH = false;

		// 	// Actualiza el contador de coyote time
		// 	if (IsOnFloor())
		// 	{
		// 		CoyoteTimeCounter = CoyoteTimeMax;
		// 	}
		// 	else
		// 	{
		// 		CoyoteTimeCounter -= (float)delta;
		// 		if (CoyoteTimeCounter < 0f)
		// 			CoyoteTimeCounter = 0f;
		// 	}
		// }

		/// <summary>
		/// Reproduce una animación en el <see cref="AnimatedSprite2D"/> hijo.
		/// Método auxiliar usado por los estados para cambiar la animación.
		/// </summary>
		/// <param name="animationName">Nombre de la animación a reproducir.</param>
		public void SetAnimation(string animationName)
		{
			if (animatedSprite != null)
			{
				animatedSprite.Play(animationName);
				GD.Print($"Setting animation to: {animationName}");
			}
		}
	}
}

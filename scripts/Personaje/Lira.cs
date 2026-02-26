using Godot;

namespace Faeterna.scripts.Player
{
    public partial class Lira : CharacterBody3D
    {
        /// <summary>Velocidad horizontal configurada del jugador (px/s).</summary>
        public const float Speed = 10.0f;

        /// <summary>Velocidad vertical aplicada al iniciar un salto (negativa = hacia arriba).</summary>
        public const float JumpVelocity = 20.0f;

        /// <summary>Gravedad usada en los estados aéreos. Equivalente a GetGravity() de 2D.</summary>
        public const float Gravity = -50.0f;

        public bool DoubleJumpAvailable = true;
        public bool DashAvailable = true;
        public bool CoyoteAvailable = true;

        public AnimatedSprite3D animatedSprite;

        [Export] TextureRect textureRect;
        [Export] TextureRect textureRect2;


        public override void _Ready()
        {
            animatedSprite = GetNode<AnimatedSprite3D>("AnimatedSprite3D");
            ((ShaderMaterial)textureRect.Material).SetShaderParameter("its_on", false);
            ((ShaderMaterial)textureRect2.Material).SetShaderParameter("its_on", true);
        }

        /// <summary>
        /// Reproduce una animación en el <see cref="AnimatedSprite3D"/> hijo.
        /// Método auxiliar usado por los estados para cambiar la animación.
        /// </summary>
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

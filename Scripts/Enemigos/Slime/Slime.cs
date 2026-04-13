using Faeterna.Scripts.Personaje;
using Godot;

namespace Faeterna.Scripts.Enemigos.Slime
{
    public partial class Slime : CharacterBody2D
    {
        /// <summary>>Fuerza vertical aplicada al iniciar un salto (negativa = hacia arriba).</summary>
        public float JumpForce = -380f;
        /// <summary>Intervalo de tiempo entre cada salto (en segundos).</summary>
        public float JumpInterval = 2.0f;
        public float HorizontalSpeed = 120f;

        private static float Gravity => ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

        private AnimatedSprite2D _animatedSprite;
        private Timer _jumpTimer;
        private bool _isJumping = false;
        private int _jumpDirection = 1; // Alterna entre -1 y 1

        private Node2D _target = null; // Si quieres perseguir al jugador

        [Export] private CollisionShape2D _detectionArea;

        public override void _Ready()
        {
            _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

            _jumpTimer = new Timer
            {
                WaitTime = JumpInterval,
                OneShot = false
            };
            _jumpTimer.Timeout += OnJumpTimerTimeout;
            AddChild(_jumpTimer);
            _jumpTimer.Start();

            set_animation("idle");
        }

        public override void _PhysicsProcess(double delta)
        {
            Vector2 velocity = Velocity;

            if (!IsOnFloor())
                velocity.Y += Gravity * (float)delta;

            if (_isJumping && IsOnFloor() && velocity.Y >= 0)
            {
                _isJumping = false;
                velocity.X = 0;
                velocity.Y = 0;
                set_animation("idle");
            }

            Velocity = velocity;
            MoveAndSlide();
        }

        private void OnJumpTimerTimeout()
        {
            if (!IsOnFloor()) return;

            _isJumping = true;

            float directionX;

            if (_target != null)
            {
                // Saltar hacia el jugador si está detectado
                directionX = Mathf.Sign(_target.GlobalPosition.X - GlobalPosition.X);
            }
            else
            {
                // Alternar de lado a lado
                directionX = _jumpDirection;
                _jumpDirection *= -1;
            }

            // Voltear sprite según dirección
            _animatedSprite.FlipH = directionX < 0;
            _detectionArea.Position = new Vector2(88.25f * directionX, 0); // Ajusta el área de detección

            Velocity = new Vector2(directionX * HorizontalSpeed, JumpForce);
            set_animation("jump");
        }

        public void set_animation(string animation_name)
        {
            _animatedSprite?.Play(animation_name);
        }

        public void _on_attack_hit_box_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
                lira.TakeDamage(1, GlobalPosition);
        }

        public void _on_detection_area_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
                _target = lira; // Empieza a perseguir al jugador
        }

        public void _on_detection_area_body_exited(Node2D prota)
        {
            if (prota is Lira)
                _target = null; // Vuelve a alternar lados
        }
    }
}

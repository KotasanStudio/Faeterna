using Godot;
using Faeterna.Scripts.Personaje;
using EnemyClass = Faeterna.Scripts.Enemigos.Enemy.Enemy;

namespace Faeterna.Scripts.Enemigos.Slime
{
    public partial class Slime : EnemyClass
    {
        /// <summary>>Fuerza vertical aplicada al iniciar un salto (negativa = hacia arriba).</summary>
        public float JumpForce = -380f;
        /// <summary>Intervalo de tiempo entre cada salto (en segundos).</summary>
        public float JumpInterval = 2.0f;
        public float HorizontalSpeed = 120f;

        private static float Gravity => ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
        public int Health = 6;
        private Timer _jumpTimer;
        private bool _isJumping = false;
        private int _jumpDirection = 1; // Alterna entre -1 y 1

        private Node2D _target = null; // Si quieres perseguir al jugador
        private float _knockbackTimer = 0f;
        private const float KnockbackDuration = 0.2f;
        private ShaderMaterial _shaderMaterial;

        [Export] private CollisionShape2D _detectionArea;
        [Export] private Area2D _hurtBox;

        /// <summary>
        /// Se llama cuando el nodo entra en la escena. Obtiene la referencia al nodo AnimatedSprite2D hijo para usar sus animaciones.
        /// </summary>
        public override void _Ready()
        {


            _jumpTimer = new Timer
            {
                WaitTime = JumpInterval,
                OneShot = false
            };
            _jumpTimer.Timeout += OnJumpTimerTimeout;
            AddChild(_jumpTimer);
            _jumpTimer.Start();
            _shaderMaterial = (ShaderMaterial)_animatedSprite.Material;
            SetAnimation("idle");
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
                SetAnimation("idle");
            }
            if (_knockbackTimer > 0f) 
            {
                velocity = Vector2.Zero; // Detener el movimiento horizontal después del knockback
            }

            Velocity = velocity;
            MoveAndSlide();
        }

        private void OnJumpTimerTimeout()
        {
            if (!IsOnFloor())
                return;

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
            SetAnimation("jump");
        }

        /// <summary>
        /// Se llama cuando algo entra en el área de ataque del slime. Verifica si es el jugador (Lira) y le inflige daño
        /// si entra en contacto con el slime detectando colisiones de física.
        /// </summary>
        /// <param name="prota">Nodo 2D que entró en el área de ataque.</param>
        public void _on_attack_hit_box_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
                lira.TakeDamage(1, GlobalPosition);
        }

        /// <summary>
        /// Se llama cuando algo entra en el área de detección del slime. Verifica si es el jugador (Lira).
        /// La lógica de respuesta a la detección está pendiente de implementar.
        /// </summary>
        /// <param name="prota">Nodo 2D que entró en el área de detección.</param>
        public void _on_detection_area_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
                _target = lira; // Empieza a perseguir al jugador
        }

        /// <summary>
        /// Se llama cuando algo sale del área de detección del slime. Verifica si es el jugador (Lira).
        /// La lógica de salida de detección está pendiente de implementar.
        /// </summary>
        /// <param name="prota">Nodo 2D que salió del área de detección.</param>
        public void _on_detection_area_body_exited(Node2D prota)
        {
            if (prota is Lira)
                _target = null; // Vuelve a alternar lados
        }
        private void OnHurtBoxAreaEntered(Area2D area)
        {
            if (area.GetParent() is Lira lira)
                TakeDamage(1, lira.GlobalPosition);

            if (area is Shot shot)
                TakeDamage((int)shot.Scale.X, shot.GlobalPosition);
        }

        private void TakeDamage(int v, Vector2 globalPosition)
        {
            Health -= v;
            hitShader(_shaderMaterial);
            GD.Print("Slime take damage: " + v + ", remaining health: " + Health);
            if (Health <= 0)
            {
                Velocity = Vector2.Zero; // Detener cualquier movimiento

                Timer timer = new Timer
                {
                    WaitTime = 0.5f,
                    OneShot = true
                };

                AddChild(timer);

                timer.Timeout += () =>
                {
                    QueueFree();
                    timer.QueueFree();
                };

                timer.Start();
            }
            _isJumping = false;
            _knockbackTimer = KnockbackDuration;

            // Dirección opuesta al atacante + mini salto
            float directionX = GlobalPosition.X >= globalPosition.X ? 1.0f : -1.0f;
            Velocity = new Vector2(directionX * 250f, -200f);
        }
    }
}

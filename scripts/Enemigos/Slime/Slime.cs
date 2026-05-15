using Godot;
using Faeterna.Scripts.Personaje;

namespace Faeterna.Scripts.Enemigos.Slime
{
    public partial class Slime : Enemy
    {
        public float JumpInterval = 2.0f;
        public float HorizontalSpeed = 120f;
        private Timer _jumpTimer;
        private bool _isJumping = false;
        private int _jumpDirection = 1; // Alterna entre -1 y 1

        public override void _EnterTree()
        {
            if (flipSprite)
            {
                animatedSprite.FlipH = flipSprite;
                detectionArea.Position = new Vector2(88.25f * -1, 0); // Ajusta el área de detección
            }
        }
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
            shaderMaterial = (ShaderMaterial)animatedSprite.Material;
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
            else if (!_isJumping && IsOnFloor())
            {
                velocity.X = 0; // Detener el movimiento horizontal cuando no está saltando
            }
            if (knockbackTimer > 0f)
            {
                knockbackTimer -= (float)delta;
                if (IsOnFloor()&&health>0)
                {
                    SetAnimation("idle");
                    velocity = Vector2.Zero; // Detener el movimiento horizontal después del knockback
                }
            }

            if(health>0)
            {
            Velocity = velocity;
            MoveAndSlide();
            }
        }

        private void OnJumpTimerTimeout()
        {
            if (!IsOnFloor() || health <= 0)
                return;

            _isJumping = true;

            float directionX;

            if (target != null)
            {
                // Saltar hacia el jugador si está detectado
                directionX = Mathf.Sign(target.GlobalPosition.X - GlobalPosition.X);
            }
            else
            {
                // Alternar de lado a lado
                directionX = _jumpDirection;
                _jumpDirection *= -1;
            }

            // Voltear sprite según dirección
            animatedSprite.FlipH = directionX < 0;
            detectionArea.Position = new Vector2(88.25f * directionX, 0); // Ajusta el área de detección

            Velocity = new Vector2(directionX * HorizontalSpeed, jumpVelocity);
            SetAnimation("jump");
        }

        public void _on_attack_hit_box_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
                lira.TakeDamage(1, GlobalPosition);
        }

        public void _on_detection_area_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
                target = lira; // Empieza a perseguir al jugador
        }

        public void _on_detection_area_body_exited(Node2D prota)
        {
            if (prota is Lira)
                target = null; // Vuelve a alternar lados
        }
        private void OnHurtBoxAreaEntered(Area2D area)
        {
            if (area.Name == "KickHitbox" && area.GetParent() is Lira lira)
                TakeDamage(1, lira.GlobalPosition);

            if (area is Shot shot)
                TakeDamage((int)shot.Scale.X, shot.GlobalPosition);
        }

        private void TakeDamage(int v, Vector2 globalPosition)
        {
            health -= v;
            hitShader(shaderMaterial);
            GD.Print("Slime take damage: " + v + ", remaining health: " + health);
            if (health <= 0)
            {
                SetAnimation("dead");
                target = null; // Deja de perseguir al jugador
                attackHitBox.SetDeferred("monitoring", false); // Desactivar el área de daño para evitar más colisiones
                hurtBox.SetDeferred("monitoring", false); // Desactivar el área de daño para evitar más colisiones
                detectionArea.GetParent<Area2D>().SetDeferred("monitoring", false); // Desactivar el área de detección para evitar más colisiones
                Velocity = Vector2.Zero; // Detener cualquier movimient0
                _isJumping = false;
                Timer timer = new Timer
                {
                    WaitTime = 0.4f,
                    OneShot = true
                };

                AddChild(timer);

                timer.Timeout += () =>
                {
                    QueueFree();
                };

                timer.Start();
            }
            else
            {

                _isJumping = false;
                knockbackTimer = knockbackDuration;

                // Dirección opuesta al atacante + mini salto
                float directionX = GlobalPosition.X >= globalPosition.X ? 1.0f : -1.0f;
                Velocity = new Vector2(directionX * 250f, -200f);
            }
        }
    }
}

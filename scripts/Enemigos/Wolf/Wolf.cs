using System;
using Faeterna.Scripts.Personaje;
using Faeterna.Scripts.Enemigos;
using Godot;

namespace Faeterna.scripts.Enemigos.Wolf
{
    public partial class Wolf : Enemy
    {
        public float DashInterval = 2f;
        private Timer _dashTimer;
        private bool _isDashing = false;
        // <summary>Timer que controla la duración del dash (cuánto tiempo dura el impulso).</summary>

        private Random _rnd = new();

        public override void _EnterTree()
        {
            if (flipSprite)
            {
                animatedSprite.FlipH = flipSprite;
                detectionArea.Position = new Vector2(88.25f * dashDirection * -1, 0); // Ajusta el área de detección
            }
        }
        public override void _Ready()
        {
            _dashTimer = new Timer
            {
                WaitTime = DashInterval,
                OneShot = false
            };
            _dashTimer.Timeout += OnDashTimerTimeout;
            AddChild(_dashTimer);
            _dashTimer.Start();
            shaderMaterial = (ShaderMaterial)animatedSprite.Material;
            SetAnimation("idle");
        }

        public override void _PhysicsProcess(double delta)
        {
            Vector2 velocity = Velocity;

            if (!IsOnFloor())
                velocity.Y += Gravity * (float)delta;

            if (_isDashing)
            {
                // Durante el dash, mantenemos la velocidad constante en la dirección del dash.
                dashDuration -= (float)delta;

                // Fuerza la actualización del raycast en cada frame
                groundCheck.ForceRaycastUpdate();

                // Para el dash si no hay suelo delante o se acabó el tiempo límite
                if ((!groundCheck.IsColliding() && target == null) || _dashTimer.WaitTime <= 0f)
                {
                    _isDashing = false;
                    velocity.X = 0;
                    SetAnimation("idle");
                }
            }

            if (knockbackTimer > 0f)
            {
                knockbackTimer -= (float)delta;
                if (IsOnFloor() && health > 0)
                {
                    SetAnimation("attack");
                    velocity.X = 0f;
                }
            }
            if(health>0)
            {
            Velocity = velocity;
            MoveAndSlide();
            }
        }
        private void OnDashTimerTimeout()
        {
            if (!IsOnFloor() || health <= 0)
                return;

            _isDashing = true;
            float directionX;

            if (target != null)
                directionX = Mathf.Sign(target.GlobalPosition.X - GlobalPosition.X);
            else
            {
                directionX = dashDirection;
                dashDirection *= -1;
            }

            animatedSprite.FlipH = directionX < 0;
            groundCheck.Position = new Vector2(Mathf.Abs(groundCheck.Position.X) * directionX, groundCheck.Position.Y); // Ajusta la posición del raycast según la dirección
            detectionArea.Position = new Vector2(156.25f * directionX, 0); // Ajusta el área de detección
            Velocity = new Vector2(directionX * dashSpeed, Velocity.Y);
            SetAnimation("run");
            playAudio("runSound");

        }

        public void _on_attack_hit_box_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
                lira.TakeDamage(1, GlobalPosition);
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
            playAudio("hitSound");
            if (health <= 0)
            {
                SetAnimation("dead");
                _isDashing = false;
                target = null; // Deja de perseguir al jugador
                attackHitBox.SetDeferred("monitoring", false); // Desactivar el área de daño para evitar más colisiones
                hurtBox.SetDeferred("monitoring", false); // Desactivar el área de daño para evitar más colisiones
                detectionArea.GetParent<Area2D>().SetDeferred("monitoring", false); // Desactivar el área de detección para evitar más colisiones
                Velocity = Vector2.Zero; // Detener cualquier movimiento

                Timer timer = new Timer
                {
                    WaitTime = 1f,
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
                _isDashing = false;
                knockbackTimer = knockbackDuration;

                // Dirección opuesta al atacante + mini salto
                float directionX = GlobalPosition.X >= globalPosition.X ? 1.0f : -1.0f;
                Velocity = new Vector2(directionX * 250f, -200f);

            }


        }

        public void _on_detection_area_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
            {

                target = lira;
            }
        }

        public void _on_detection_area_body_exited(Node2D prota)
        {
            if (prota is Lira)
                target = null;
        }


    }
}

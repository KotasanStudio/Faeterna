using Godot;
using Faeterna.Scripts.Personaje;

namespace Faeterna.Scripts.Enemigos.Slime
{
    /// <summary>
    /// Enemigo tipo Slime que salta periódicamente. Si detecta al jugador, salta hacia él.
    /// Si no hay objetivo, alterna saltando de lado a lado. Recibe daño por colisiones
    /// con ataques del jugador y proyectiles.
    /// </summary>
    public partial class Slime : Enemy
    {
        /// <summary>Intervalo de tiempo entre saltos en segundos.</summary>
        public float JumpInterval = 2.0f;

        /// <summary>Velocidad horizontal del slime durante el salto (px/s).</summary>
        public float HorizontalSpeed = 120f;

        /// <summary>Timer que controla los saltos periódicos del slime.</summary>
        private Timer _jumpTimer;

        /// <summary>Indica si el slime está actualmente en el aire saltando.</summary>
        private bool _isJumping = false;

        /// <summary>Dirección del siguiente salto (alterna entre -1 y 1 cuando no hay objetivo).</summary>
        private int _jumpDirection = 1; // Alterna entre -1 y 1

        [ExportGroup("Audio")]
        /// <summary>Sonido de salto del slime. Se asigna desde el editor.</summary>
        [Export] private AudioStream _jumpAudio;

        /// <summary>Sonido de daño del slime. Se asigna desde el editor.</summary>
        [Export] private AudioStream _hitAudio;

        /// <summary>
        /// Se llama cuando el nodo entra en el árbol (antes de _Ready).
        /// Configura la orientación inicial del sprite si flipSprite está activado.
        /// </summary>
        public override void _EnterTree()
        {
            if (flipSprite)
            {
                animatedSprite.FlipH = flipSprite;
                detectionArea.Position = new Vector2(88.25f * -1, 0); // Ajusta el área de detección
            }
        }

        /// <summary>
        /// Inicialización del slime. Crea el timer de saltos, lo conecta a su callback
        /// y obtiene el material del shader para efectos de daño.
        /// </summary>
        public override void _Ready()
        {

            if(_jumpAudio==null)
                _jumpAudio = GD.Load<AudioStream>("res://assets/Audio/Slime/SlimeJump.wav");

            if(_hitAudio==null)
                _hitAudio = GD.Load<AudioStream>("res://assets/Audio/Slime/SlimeHit.wav");


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

        /// <summary>
        /// Procesamiento de física cada frame. Gestiona la gravedad, el movimiento horizontal
        /// durante el salto y el knockback.
        /// </summary>
        /// <param name="delta">Tiempo en segundos desde el último frame.</param>
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

        /// <summary>
        /// Maneja el timeout del timer de saltos. Si el jugador fue detectado, el slime
        /// salta hacia él. Si no, alterna saltando de lado a lado.
        /// </summary>
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
            playAudio(_jumpAudio);

        }

        /// <summary>
        /// Maneja la colisión del hitbox de ataque con otros cuerpos.
        /// Si el cuerpo es el jugador, le inflige daño.
        /// </summary>
        /// <param name="prota">Nodo que colisionó con el hitbox.</param>
        public void _on_attack_hit_box_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
                lira.TakeDamage(1, GlobalPosition);
        }

        /// <summary>
        /// Maneja la detección del jugador. Establece el objetivo para que el slime
        /// salte hacia él en los próximos saltos.
        /// </summary>
        /// <param name="prota">Nodo que entró en el área de detección.</param>
        public void _on_detection_area_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
                target = lira; // Empieza a perseguir al jugador
        }

        /// <summary>
        /// Maneja cuando el jugador sale del área de detección.
        /// El slime vuelve a alternar saltando de lado a lado.
        /// </summary>
        /// <param name="prota">Nodo que salió del área de detección.</param>
        public void _on_detection_area_body_exited(Node2D prota)
        {
            if (prota is Lira)
                target = null; // Vuelve a alternar lados
        }

        /// <summary>
        /// Maneja la colisión del hurtbox (área de daño recibido) con áreas de ataque.
        /// Aplica daño al slime si es golpeado por el jugador o un proyectil.
        /// </summary>
        /// <param name="area">Área que colisionó con el hurtbox.</param>
        private void OnHurtBoxAreaEntered(Area2D area)
        {
            if (area.Name == "KickHitbox" && area.GetParent() is Lira lira)
                TakeDamage(1, lira.GlobalPosition);

            if (area is Shot shot)
                TakeDamage((int)(shot.Scale.X *1.5f+1), shot.GlobalPosition);
        }

        /// <summary>
        /// Reduce la salud del slime, aplica efectos visuales y de sonido, y maneja la muerte.
        /// </summary>
        /// <param name="v">Cantidad de daño a aplicar.</param>
        /// <param name="globalPosition">Posición del atacante, utilizada para determinar dirección del knockback.</param>
        private void TakeDamage(int v, Vector2 globalPosition)
        {
            health -= v;
            hitShader(shaderMaterial);
            playAudio(_hitAudio);
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



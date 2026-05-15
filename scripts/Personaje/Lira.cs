
using Godot;
using Godot.Collections;
using Faeterna.Scripts.Tools;
using System.Threading.Tasks;
using Faeterna.Scripts.Menus;
using Faeterna.scripts.Personaje;
using Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento;

namespace Faeterna.Scripts.Personaje
{
    /// <summary>
    /// Personaje principal del juego (Lira).
    /// Gestiona salud, maná, animaciones y las banderas de movimiento
    /// utilizadas por la máquina de estados.
    /// </summary>
    public partial class Lira : CharacterBody2D
    {
        /// <summary>Velocidad horizontal configurada del jugador (px/s).</summary>
        public const float Speed = 350.0f;

        /// <summary>Velocidad vertical aplicada al iniciar un salto (negativa = hacia arriba).</summary>
        public const float JumpVelocity = -500.0f;

        /// <summary>Gravedad usada en los estados aéreos. Se obtiene del ProjectSettings.</summary>
        public static float Gravity => ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

        /// <summary>Puntos de vida máximos del personaje.</summary>
        private const int Health = 5;

        /// <summary>Maná máximo del personaje.</summary>
        private const int Mana = 100;

        /// <summary>Puntos de vida actuales del personaje.</summary>
        public int _currentHealth = Health;

        /// <summary>Maná actual del personaje.</summary>
        public float _currentMana = Mana;

        public Vector2 cameraInitPos;

        /// <summary>Indica si el doble salto está disponible.</summary>
        public bool DoubleJumpAvailable = true;

        /// <summary>Indica si el dash está disponible.</summary>
        public bool DashAvailable = true;

        /// <summary>Indica si el coyote time está disponible (permite saltar brevemente tras abandonar el suelo).</summary>
        public bool CoyoteAvailable = true;

        /// <summary>Fuerza horizontal del knockback al recibir daño.</summary>
        private const float KnockbackForceX = 300.0f;

        /// <summary>Fuerza vertical del knockback al recibir daño (negativa = hacia arriba).</summary>
        private const float KnockbackForceY = -250.0f;

        /// <summary>Referencia al nodo <see cref="AnimatedSprite2D"/> hijo que muestra las animaciones del personaje.</summary>
        public AnimatedSprite2D animatedSprite;

        /// <summary>Referencia a la máquina de estados de movimiento.</summary>
        public MovementStateMachine MovementStateMachine;

        /// <summary>Controlador del AnimationTree — sincroniza animaciones con el estado de movimiento.</summary>
        public LiraAnimationTree AnimTree;
        public bool ItsFliped = false;
        public bool IsInvulnerableByState = false;
        /// <summary>Lista de <see cref="TextureRect"/> que representan los corazones de vida en la interfaz.</summary>
        [Export] public Array<TextureRect> _hearts;

        /// <summary><see cref="TextureRect"/> que representa la barra de maná en la interfaz.</summary>
        [Export] public TextureRect _manaBar;

        /// <summary>Timer que controla la duración de la invencibilidad tras recibir daño.</summary>
        [Export] private Timer _invencibilityTimer;

        [ExportGroup("Atacks")]
        [Export] private Area2D _shotArea;
        [Export] private Area2D _kickArea;

        [Export] private PackedScene _bullet;


        [ExportGroup("Particles")]
        [Export] public TerrainParticles terrainParticles;

        [Export] public CpuParticles2D  saltoParticulas;

        [Export] public CpuParticles2D  dobleSaltoParticulas;

        private bool _tutorial = false;

        [Export] public DeathScreen  _deathScreen;

        /// <summary>
        /// Inicialización del nodo. Obtiene la referencia al <see cref="AnimatedSprite2D"/> hijo.
        /// </summary>
        public override async void _Ready()
        {
            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            MovementStateMachine = GetNode<MovementStateMachine>("MovementStateMachine");
            var animTreeNode = GetNodeOrNull<AnimationTree>("AnimationTree");
            if (animTreeNode != null)
                AnimTree = GetNodeOrNull<LiraAnimationTree>("AnimationTree/LiraAnimationTree");
            UpdateHearts();
            UpdateMana();
            await TryLoadFromActiveSlotAsync();

        }

        private async Task TryLoadFromActiveSlotAsync()
        {
            GameData gameData = await GameSaveService.LoadActiveSlotAsync();
            if (gameData?.PlayerData == null)
            {
                return;
            }

            string currentScenePath = GetTree().CurrentScene?.SceneFilePath ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(gameData.ScenePath)
                && !string.Equals(gameData.ScenePath, currentScenePath, System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            ApplySaveData(gameData.PlayerData);
        }

        public PlayerSaveData BuildSaveData(Vector2 position)
        {
            return new PlayerSaveData
            {
                Position = position,
                Health = _currentHealth,
                Mana = _currentMana,
                // NO guardamos las flags de movimiento (DoubleJump, Dash, Coyote) porque
                // son transitorias y se manejan exclusivamente por la máquina de estados.
                // Guardarlas causaría inconsistencias cuando el jugador carga.
                DoubleJumpAvailable = true,
                DashAvailable = true,
                CoyoteAvailable = true
            };
        }

        public void ApplySaveData(PlayerSaveData saveData)
        {
            GlobalPosition = saveData.Position;
            Velocity = Vector2.Zero;

            _currentHealth = Mathf.Clamp(saveData.Health, 0, Health);
            _currentMana = Mathf.Clamp(saveData.Mana, 0f, Mana);

            // IMPORTANTE: Los flags de movimiento (DoubleJumpAvailable, DashAvailable, CoyoteAvailable)
            // SIEMPRE se resetean a true al cargar, independientemente del valor guardado.
            // Razones:
            // 1. Estos flags son transitorios y se manejan completamente por la máquina de estados
            // 2. Guardarlos causaría comportamientos no deseados (triple saltos, saltos infinitos)
            // 3. Cuando el jugador carga en un checkpoint, comienza con un estado limpio
            // 4. El flag CoyoteAvailable será controlado por RunningMovementState (línea 72)
            // 5. El flag DoubleJumpAvailable será restaurado por FallingMovementState (línea 79)
            DoubleJumpAvailable = true;
            DashAvailable = true;
            CoyoteAvailable = true;

            UpdateHearts();
            UpdateMana();
        }

        /// <summary>
        /// Reproduce una animación en el <see cref="AnimatedSprite2D"/> hijo.
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

        /// <summary>
        /// Aplica daño al personaje, reduciendo su salud y reproduciendo un efecto
        /// de flash en los corazones de la interfaz.
        /// </summary>
        /// <param name="amount">Cantidad de puntos de vida a restar.</param>
        /// <param name="attackerPosition">Posición mundial del atacante, usada para calcular la dirección del knockback.</param>
        public async void TakeDamage(int amount, Vector2 attackerPosition)
        {
            if (_invencibilityTimer.IsStopped() == false || IsInvulnerableByState || _currentHealth <= 0)
            {
                return;
            }
            _currentHealth -= amount;
            UpdateHearts();

            if (_currentHealth <= 0)
            {
                // El control se bloquea AQUÍ
                MovementStateMachine?.TransitionTo("DeathMovementState");
                return;
            }

            // Si no ha muerto, aplicamos el knockback normal
            float directionX = GlobalPosition.X >= attackerPosition.X ? 1.0f : -1.0f;
            Velocity = new Vector2(directionX * KnockbackForceX, KnockbackForceY);
            MovementStateMachine?.TransitionTo("KnockbackMovementState");

            _invencibilityTimer.Start();
        }

        /// <summary>
        /// Actualiza visualmente los corazones de la interfaz según la salud actual.
        /// </summary>
        private void UpdateHearts()
        {
            if (_currentHealth == 0)
            {
                AnimTree?.Travel("dead");
                MovementStateMachine?.TransitionTo("DeadMovementState");
            }
            for (int i = 0; i < _hearts.Count; i++)
            {
                if (_hearts[i].Material is ShaderMaterial mat)
                {
                    if (i < _currentHealth)
                        mat.SetShaderParameter("fill_amount", 1.0f);
                    else
                        mat.SetShaderParameter("fill_amount", 0.0f);
                }
            }
        }

        /// <summary>
        /// Cura al personaje, aumentando su salud hasta el máximo permitido
        /// y actualizando la interfaz.
        /// </summary>
        /// <param name="amount">Cantidad de puntos de vida a restaurar.</param>
        public void Heal(int amount)
        {
            _currentHealth += amount;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, Health);
            UpdateHearts();
        }

        /// <summary>
        /// Actualiza visualmente la barra de maná de la interfaz según el maná actual.
        /// </summary>
        private void UpdateMana()
        {
            float percent = _currentMana / Mana;
            if (_manaBar.Material is ShaderMaterial mat)
                mat.SetShaderParameter("fill_amount", percent);
        }

        /// <summary>
        /// Consume maná del personaje y actualiza la barra de la interfaz.
        /// </summary>
        /// <param name="amount">Cantidad de maná a gastar.</param>
        public void UseMana(float amount)
        {
            _currentMana -= amount;
            _currentMana = Mathf.Clamp(_currentMana, 0, Mana);
            GD.Print(_currentMana);
            UpdateMana();
        }

        /// <summary>
        /// Recupera maná del personaje hasta el máximo permitido
        /// y actualiza la barra de la interfaz.
        /// </summary>
        /// <param name="amount">Cantidad de maná a recuperar.</param>
        public void RecoverMana(float amount)
        {
            _currentMana += amount;
            _currentMana = Mathf.Clamp(_currentMana, 0, Mana);
            UpdateMana();
        }

        public void FlipH(bool value)
        {
            ItsFliped = value;
            animatedSprite.FlipH = value;
            if (value)
            {
                _shotArea.Position = new Vector2(-Mathf.Abs(_shotArea.Position.X), _shotArea.Position.Y);
                _kickArea.GetNode<CollisionShape2D>("KickHitbox1").Position = new Vector2(-Mathf.Abs(_kickArea.GetNode<CollisionShape2D>("KickHitbox1").Position.X), _kickArea.GetNode<CollisionShape2D>("KickHitbox1").Position.Y);
                _kickArea.GetNode<CollisionShape2D>("KickHitbox1").RotationDegrees = -45f;
                _kickArea.GetNode<CollisionShape2D>("KickHitbox2").Position = new Vector2(-Mathf.Abs(_kickArea.GetNode<CollisionShape2D>("KickHitbox2").Position.X), _kickArea.GetNode<CollisionShape2D>("KickHitbox2").Position.Y);
            }
            else
            {
                _shotArea.Position = new Vector2(Mathf.Abs(_shotArea.Position.X), _shotArea.Position.Y);
                _kickArea.GetNode<CollisionShape2D>("KickHitbox1").Position = new Vector2(Mathf.Abs(_kickArea.GetNode<CollisionShape2D>("KickHitbox1").Position.X), _kickArea.GetNode<CollisionShape2D>("KickHitbox1").Position.Y);
                _kickArea.GetNode<CollisionShape2D>("KickHitbox1").RotationDegrees = 45f;
                _kickArea.GetNode<CollisionShape2D>("KickHitbox2").Position = new Vector2(Mathf.Abs(_kickArea.GetNode<CollisionShape2D>("KickHitbox2").Position.X), _kickArea.GetNode<CollisionShape2D>("KickHitbox2").Position.Y);
            }
        }
        public void Shooting(double manaCost, double shotBallScale)
        {
            var InstanciaShot = (Shot)_bullet.Instantiate();
            InstanciaShot.ManaCost = (float)manaCost;
            InstanciaShot.Scale = new Vector2((float)shotBallScale, (float)shotBallScale);

            if ((_currentMana - InstanciaShot.ManaCost) >= 0)
            {
                // CAMBIO AQUÍ: En lugar de SetAnimation, usa el AnimTree
                AnimTree?.Travel("shot");

                UseMana(InstanciaShot.ManaCost);
                if (ItsFliped)
                    InstanciaShot.Direction = new Vector2(-1, 0);
                else
                    InstanciaShot.Direction = new Vector2(1, 0);
                InstanciaShot.GlobalPosition = _shotArea.GlobalPosition;
                GetTree().CurrentScene?.AddChild(InstanciaShot);            }
        }

       public void OnKickHitboxAreaEntered(Area2D area)
        {
            GD.Print("Kick hitbox activated");
            RecoverMana(10f);
        }
    }
}

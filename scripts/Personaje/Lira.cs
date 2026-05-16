
using Godot;
using Godot.Collections;
using Faeterna.Scripts.Tools;
using System.Threading.Tasks;
using Faeterna.Scripts.Menus;
using Faeterna.scripts.Personaje;
using Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento;
using System;
using System.ComponentModel;

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
        public const float JumpVelocity = -445.0f;

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

        [Export] private Camera2D _camera;

        /// <summary>Lista de <see cref="TextureRect"/> que representan los corazones de vida en la interfaz.</summary>
        [Export] public Array<TextureRect> _hearts;

        /// <summary><see cref="TextureRect"/> que representa la barra de maná en la interfaz.</summary>
        [Export] public TextureRect _manaBar;

        /// <summary>Timer que controla la duración de la invencibilidad tras recibir daño.</summary>
        [Export] private Timer _invencibilityTimer;

        /// <summary>
        /// Áreas usadas para detectar colisiones de ataques (disparo y patada).
        /// </summary>
        [ExportGroup("Atacks")]
        [Export] private Area2D _shotArea;
        [Export] private Area2D _kickArea;


        /// <summary>Escena del proyectil disparado por el personaje. Se instancia al atacar y se le asignan propiedades como dirección y posición.</summary>
        [Export] private PackedScene _bullet;

        /// <summary>
        /// Nodo que emite partículas al caminar sobre el terreno. Se activa/desactiva desde los estados de movimiento para mejorar la inmersión visual.
        /// </summary>
        [ExportGroup("Particles")]
        [Export] public TerrainParticles terrainParticles;
        [Export] public CpuParticles2D saltoParticulas;
        [Export] public CpuParticles2D dobleSaltoParticulas;

        /// <summary>Flags de tutorial. Estas variables se usan para controlar eventos específicos del tutorial, como la aparición de mensajes o la activación de mecánicas. No afectan directamente a la jugabilidad, pero permiten personalizar la experiencia durante el tutorial.</summary>
        private bool _tutorial = false;

        /// <summary>Flags de habilidades. Estas variables indican si el personaje ha adquirido ciertas habilidades (doble salto, dash) a lo largo del juego. Son usadas por la máquina de estados para determinar qué acciones están disponibles para el jugador en cada momento. Aunque podrían ser parte de un sistema de progresión más complejo, en este caso se manejan directamente en el personaje para simplificar su acceso desde los estados de movimiento.</summary>
        public bool _haveDobleJump { get; set;} = true;

        /// <summary>Flags de habilidades. Estas variables indican si el personaje ha adquirido ciertas habilidades (doble salto, dash) a lo largo del juego. Son usadas por la máquina de estados para determinar qué acciones están disponibles para el jugador en cada momento. Aunque podrían ser parte de un sistema de progresión más complejo, en este caso se manejan directamente en el personaje para simplificar su acceso desde los estados de movimiento.</summary>
        public bool _haveDash { get; set;} = true;

        /// <summary>Referencias a nodos de la interfaz y otros elementos relacionados con la salud, maná, muerte y descripción de objetos. Estos nodos se asignan desde el editor y se usan para actualizar visualmente la interfaz (corazones, barra de maná) y mostrar pantallas de muerte o descripciones de objetos cuando sea necesario. Aunque podrían estar gestionados por un sistema de UI separado, en este caso se incluyen directamente en el personaje para facilitar su acceso desde los estados de movimiento y otros métodos del personaje.</summary>
        [Export] public DeathScreen  _deathScreen;

        /// <summary>Nodo que muestra la descripción de objetos al interactuar con ellos. Se asigna desde el editor y se usa para mostrar información relevante sobre objetos del juego (nombre, descripción, efectos) cuando el jugador interactúa con ellos. Aunque podría ser parte de un sistema de interacción más amplio, en este caso se incluye directamente en el personaje para facilitar su acceso desde los estados de movimiento y otros métodos relacionados con la interacción.</summary>
        [Export] public ObjetoDescription _objectoDescription;

        /// <summary>
        /// Referencias a nodos relacionados con el audio del personaje. El <see cref="AudioStreamPlayer2D"/> se usa para reproducir sonidos asociados a las acciones del personaje (correr, saltar, atacar, recibir daño). Los <see cref="AudioStream"/> representan los diferentes clips de audio que se asignan desde el editor y se reproducen según la acción realizada. Aunque podrían estar gestionados por un sistema de audio separado, en este caso se incluyen directamente en el personaje para facilitar su acceso desde los estados de movimiento y otros métodos relacionados con las acciones del personaje.
        /// </summary>
        [ExportGroup("Audios")]
        [Export] public AudioStreamPlayer2D audioPlayer;
        [Export] public AudioStream _hitAudio;
        [Export] public AudioStream _runAudio;
        [Export] public AudioStream _jumpAudio;
        [Export] public AudioStream _fallAudio;
        [Export] public AudioStream _attackAudio;
        [Export] public AudioStream _fireBallAudio;
        [Export] public AudioStream _dashAudio;

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

        /// <summary>
        /// Intenta cargar el progreso guardado desde la ranura activa. Si se encuentra un guardado válido, se aplica al personaje. Si el guardado corresponde a una escena diferente a la actual, no se carga para evitar inconsistencias.
        /// </summary>
        private async Task TryLoadFromActiveSlotAsync()
        {
            // Si hay datos pendientes fruto de una recarga de escena anterior, aplicarlos y limpiar el pending
            var pending = GameSaveService.RetrieveAndClearPending();
            if (pending?.PlayerData != null)
            {
                ApplySaveData(pending.PlayerData);
                // Eliminar bosses derrotados según el guardado (persistencia de bosses)
                RemoveDefeatedBossesFromScene(pending.DefeatedBossTypes);
                // Restaurar también el estado en memoria del GameSaveService
                GameSaveService.SetDefeatedBosses(pending.DefeatedBossTypes);
                return;
            }

            // Sólo cargar el guardado activo si la transición lo ha pedido explícitamente.
            if (!GameSaveService.ConsumeLoadActiveSlotOnNextScene())
            {
                return;
            }

            GameData gameData = await GameSaveService.LoadActiveSlotAsync();
            if (gameData?.PlayerData == null)
            {
                return;
            }

            string currentScenePath = GetTree().CurrentScene?.SceneFilePath ?? string.Empty;

            // Queremos que, al cargar una partida desde un checkpoint, la escena se recargue
            // para restaurar el estado por defecto de los enemigos (no-boss) y objetos.
            // Para ello guardamos los datos pendientes y forzamos ChangeSceneToFile al mismo
            // fichero de escena. Cuando la nueva escena termine de cargarse, su Lira
            // recuperará los datos pendientes y los aplicará.
            GameSaveService.SetPendingGameData(gameData);
            try
            {
                // Si la ruta de escena está vacía, recargamos la escena actual para reiniciar entidades.
                string sceneToLoad = string.IsNullOrWhiteSpace(gameData.ScenePath) ? currentScenePath : gameData.ScenePath;
                if (!string.IsNullOrWhiteSpace(sceneToLoad))
                {
                    GetTree().ChangeSceneToFile(sceneToLoad);
                }
            }
            catch (Exception ex)
            {
                GD.PushWarning($"No se pudo recargar la escena al aplicar guardado: {ex.Message}");
                // Como fallback, aplicamos directamente los datos sin recargar la escena
                GameSaveService.RetrieveAndClearPending();
                ApplySaveData(gameData.PlayerData);
            }
        }

        /// <summary>
        /// Construye un objeto <see cref="PlayerSaveData"/> con el estado actual del personaje, incluyendo posición, salud, maná, habilidades adquiridas y flags de movimiento. Este método se usa para guardar el progreso del jugador en checkpoints o al salir del juego. Es importante destacar que las flags de movimiento (DoubleJumpAvailable, DashAvailable, CoyoteAvailable) se guardan como true por defecto, ya que son transitorias y se manejan exclusivamente por la máquina de estados. Sin embargo, las habilidades adquiridas (HasDoubleJump, HasDash) se guardan con su valor actual para persistir el progreso del jugador en la adquisición de habilidades. Guardar las flags de movimiento con su valor actual podría causar inconsistencias al cargar el juego, como permitir saltos infinitos o dashes múltiples si el jugador guardó mientras tenía estas habilidades disponibles.
        /// </summary>
        /// <param name="position">
        /// Posición mundial del personaje que se guardará en el progreso. Esta posición se usa para colocar al personaje correctamente al cargar el juego desde un checkpoint o al restaurar el progreso. Es importante que esta posición sea precisa y corresponda a un punto válido en la escena para evitar problemas de colisiones o ubicaciones no deseadas al cargar el juego. Generalmente, esta posición se obtiene del nodo del personaje (GlobalPosition) al momento de guardar el progreso.
        /// </param>
        /// <returns>
        /// Un objeto <see cref="PlayerSaveData"/> que contiene la información necesaria para restaurar el estado del personaje al cargar el juego. Este objeto incluye la posición, salud, maná, habilidades adquiridas y flags de movimiento (aunque estas últimas se guardan como true por defecto). El objeto resultante se serializa y se almacena en el sistema de guardado para su posterior recuperación. Al cargar el juego, se aplicará esta información al personaje para restaurar su estado de manera consistente con el momento en que se guardó.
        /// </returns>
        public PlayerSaveData BuildSaveData(Vector2 position)
        {
            return new PlayerSaveData
            {
                Position = position,
                Health = _currentHealth,
                Mana = _currentMana,
                // Guardamos las habilidades adquiridas para persistir el progreso
                HasDoubleJump = _haveDobleJump,
                HasDash = _haveDash,
                // Guardar si el jugador completó/avanzó en los tutoriales
                HasCompletedTutorial = _tutorial,
                // NO guardamos las flags de movimiento (DoubleJump, Dash, Coyote) porque
                // son transitorias y se manejan exclusivamente por la máquina de estados.
                // Guardarlas causa que el personaje vuele hasta el infinito y mas alla.
                DoubleJumpAvailable = true,
                DashAvailable = true,
                CoyoteAvailable = true
            };
        }

        /// <summary>
        /// Aplica los datos de guardado al personaje, restaurando su posición, salud, maná, habilidades adquiridas y reseteando las flags de movimiento a true. Este método se usa al cargar el juego desde un checkpoint o al restaurar el progreso para colocar al personaje en el estado correcto. Es importante destacar que las flags de movimiento (DoubleJumpAvailable, DashAvailable, CoyoteAvailable) se resetean a true independientemente del valor guardado, ya que son transitorias y se manejan exclusivamente por la máquina de estados. Sin embargo, las habilidades adquiridas (HasDoubleJump, HasDash) se restauran con su valor guardado para persistir el progreso del jugador en la adquisición de habilidades. Guardar las flags de movimiento con su valor actual podría causar inconsistencias al cargar el juego, como permitir saltos infinitos o dashes múltiples si el jugador cargó mientras tenía estas habilidades disponibles. Al aplicar los datos de guardado, se garantiza un estado limpio y consistente para el personaje al cargar desde cualquier punto del juego.
        /// </summary>
        /// <param name="saveData">
        /// Un objeto <see cref="PlayerSaveData"/> que contiene la información guardada del personaje, incluyendo posición, salud, maná, habilidades adquiridas y flags de movimiento. Este objeto se obtiene al cargar el juego desde un checkpoint o al restaurar el progreso. Al aplicar esta información al personaje, se restaurará su estado de manera consistente con el momento en que se guardó, asegurando que la posición, salud, maná y habilidades sean correctos. Es importante destacar que las flags de movimiento (DoubleJumpAvailable, DashAvailable, CoyoteAvailable) se resetean a true independientemente del valor guardado para evitar inconsistencias en la jugabilidad al cargar el juego.
        /// </param>
        public void ApplySaveData(PlayerSaveData saveData)
        {
            GlobalPosition = saveData.Position;
            Velocity = Vector2.Zero;

            _currentHealth = Mathf.Clamp(saveData.Health, 0, Health);
            _currentMana = Mathf.Clamp(saveData.Mana, 0f, Mana);

            // Restauramos las habilidades adquiridas para persistir el progreso del jugador
            _haveDobleJump = saveData.HasDoubleJump;
            _haveDash = saveData.HasDash;
            // Restaurar estado de tutoriales
            _tutorial = saveData.HasCompletedTutorial;

            // IMPORTANTE: Los flags de movimiento
            // SIEMPRE se resetean a true al cargar, independientemente del valor guardado.
            DoubleJumpAvailable = true;
            DashAvailable = true;
            CoyoteAvailable = true;

            UpdateHearts();
            UpdateMana();
            // Tras aplicar los datos del jugador, eliminar objetos ya recogidos para
            // evitar que reaparezcan en la escena recargada.
            RemoveCollectedObjectsFromScene();
        }

        /// <summary>
        /// Elimina de la escena los objetos que ya han sido recogidos por el jugador
        /// para evitar que reaparezcan tras recargar la escena desde un guardado.
        /// </summary>
        private void RemoveCollectedObjectsFromScene()
        {
            var root = GetTree().CurrentScene;
            if (root == null) return;
            RemoveCollectedObjectsRecursive(root);
        }

        private void RemoveCollectedObjectsRecursive(Node node)
        {
            foreach (Node child in node.GetChildren())
            {
                // Comprueba si es un Objeto del mapa
                if (child is Faeterna.scripts.Mapa.Objeto mapaObjeto)
                {
                    int id = mapaObjeto.GetItemId();
                    if ((id == 0 && _haveDobleJump) || (id == 1 && _haveDash))
                    {
                        mapaObjeto.QueueFree();
                        continue;
                    }
                }

                RemoveCollectedObjectsRecursive(child);
            }
        }

        /// <summary>
        /// Elimina de la escena los bosses cuyos identificadores aparecen en la lista de derrotados
        /// del guardado (para que no reaparezcan tras recargar la escena si han sido vencidos).
        /// </summary>
        /// <param name="defeatedTypes">Lista de nombres de tipos/identificadores de bosses derrotados.</param>
        private void RemoveDefeatedBossesFromScene(System.Collections.Generic.List<string> defeatedTypes)
        {
            if (defeatedTypes == null || defeatedTypes.Count == 0) return;
            var root = GetTree().CurrentScene;
            if (root == null) return;
            RemoveDefeatedBossesRecursive(root, defeatedTypes);
        }

        private void RemoveDefeatedBossesRecursive(Node node, System.Collections.Generic.List<string> defeatedTypes)
        {
            foreach (Node child in node.GetChildren())
            {
                var typeName = child.GetType().Name;
                if (defeatedTypes.Contains(typeName) && child is Faeterna.Scripts.Enemigos.Enemy)
                {
                    child.QueueFree();
                    continue;
                }

                RemoveDefeatedBossesRecursive(child, defeatedTypes);
            }
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
        /// Reproduce un sonido específico según el nombre proporcionado. Este método se usa para reproducir los diferentes efectos de sonido asociados a las acciones del personaje (correr, saltar, atacar, recibir daño). El método asigna el clip de audio correspondiente al <see cref="AudioStreamPlayer2D"/> y lo reproduce. Si se proporciona un nombre de audio desconocido, se imprime un error en la consola. Es importante asegurarse de que el <see cref="AudioStreamPlayer2D"/> esté correctamente asignado en el editor para evitar errores al intentar reproducir sonidos.
        /// </summary>
        /// <param name="audioName">
        /// Nombre del audio a reproducir. Este nombre se usa para identificar qué clip de audio asignar al <see cref="AudioStreamPlayer2D"/> antes de reproducirlo. Los nombres válidos son: "run", "jump", "fall", "attack", "fireball", "dash", "hit" e "idle". Cada nombre corresponde a un clip de audio específico que debe ser asignado en el editor. Si se proporciona un nombre que no coincide con ninguno de los casos definidos, se imprimirá un error indicando que el nombre de audio es desconocido.
        /// </param>
        public void PlayAudio(string audioName)
        {
            if (audioPlayer == null)
            {
                GD.PrintErr("AudioStreamPlayer2D no asignado en Lira");
                return;
            }

            switch (audioName)
            {
                case "run":
                    audioPlayer.Stream = _runAudio;

                    break;
                case "jump":
                    audioPlayer.Stream = _jumpAudio;
                    break;
                case "fall":
                    audioPlayer.Stream = _fallAudio;
                    break;
                case "attack":
                    audioPlayer.Stream = _attackAudio;
                    break;
                case "fireball":
                    audioPlayer.Stream = _fireBallAudio;
                    break;
                case "dash":
                    audioPlayer.Stream = _dashAudio;
                    break;
                case "hit":
                    audioPlayer.Stream = _hitAudio;
                    break;
                    case "idle":
                    audioPlayer.Stream = null;
                    break;
                default:
                    GD.PrintErr($"Nombre de audio desconocido: {audioName}");
                    return;
            }
            audioPlayer.VolumeDb = +5f;
            audioPlayer.Play();
        }

        /// <summary>
        /// Aplica daño al personaje, reduciendo su salud y reproduciendo un efecto de flash en los corazones de la interfaz.
        /// </summary>
        /// <param name="amount">Cantidad de puntos de vida a restar.</param>
        /// <param name="attackerPosition">Posición mundial del atacante, usada para calcular la dirección del knockback.</param>
        public async void TakeDamage(int amount, Vector2 attackerPosition)
        {
            if (_invencibilityTimer.IsStopped() == false || IsInvulnerableByState || _currentHealth <= 0)
            {
                return;
            }
            PlayAudio("hit");
            _currentHealth -= amount;
            UpdateHearts();
            if (_currentHealth <= 0)
            {
                MovementStateMachine?.TransitionTo("DeathMovementState");
                return;
            }

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
        /// Cura al personaje, aumentando su salud hasta el máximo permitido y actualizando la interfaz.
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
        /// Recupera maná del personaje hasta el máximo permitido y actualiza la barra de la interfaz.
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
                PlayAudio("fireball");
                // CAMBIO AQUÍ: En lugar de SetAnimation, usa el AnimTree
                AnimTree?.Travel("shot");

                UseMana(InstanciaShot.ManaCost);
                if (ItsFliped)
                    InstanciaShot.Direction = new Vector2(-1, 0);
                else
                    InstanciaShot.Direction = new Vector2(1, 0);
                InstanciaShot.GlobalPosition = _shotArea.GlobalPosition;
                GetTree().CurrentScene?.AddChild(InstanciaShot);
            }
        }

        public void OnKickHitboxAreaEntered(Area2D area)
        {
            GD.Print("Kick hitbox activated");
            RecoverMana(10f);
        }

        public void GiveDoubleJump()
        {
            _haveDobleJump = true;
        }
        public void GiveDash()
        {
            _haveDash = true;
        }
        public bool HasDoubleJump() => _haveDobleJump;
        public bool HasDash() => _haveDash;
    }
}

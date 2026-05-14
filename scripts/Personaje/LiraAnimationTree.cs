using Godot;

namespace Faeterna.Scripts.Personaje
{
    /// <summary>
    /// Controlador del AnimationTree de Lira.
    ///
    /// SETUP EN GODOT:
    /// 1. Añade un nodo AnimationTree como hijo de Lira2D.
    /// 2. En el inspector:
    ///    - Tree Root  → AnimationNodeStateMachine
    ///    - Anim Player → ../AnimationPlayer
    ///    - Active      → true
    /// 3. Crea los estados en el StateMachine:
    ///    idle | run | jump | fall | dash | aim | shot | kick | getHit
    /// 4. Añade este script al nodo AnimationTree.
    ///
    /// TRANSICIONES RECOMENDADAS EN EL EDITOR:
    ///   idle  ──► run       (auto, condición: blend_position != 0)
    ///   run   ──► idle      (auto)
    ///   *     ──► jump      (auto, se controla desde código)
    ///   jump  ──► fall      (auto)
    ///   fall  ──► idle/run  (auto, al aterrizar)
    ///   *     ──► dash      (auto)
    ///   dash  ──► idle/run  (auto)
    ///   idle  ──► aim       (auto)
    ///   run   ──► aim       (auto)
    ///   aim   ──► idle      (auto)
    ///   *     ──► kick      (OneShot encima del estado base)
    ///   *     ──► getHit    (auto, se dispara desde TakeDamage)
    /// </summary>
    public partial class LiraAnimationTree : Node
    {
        // ── Referencias ──────────────────────────────────────────────
        private Lira _lira;
        private AnimationTree _animTree;
        private AnimationNodeStateMachinePlayback _stateMachine;

        // ── Nombre de nodos en el StateMachine ───────────────────────
        private const string StateIdle    = "idle";
        private const string StateRun     = "run";
        private const string StateJump    = "jump";
        private const string StateFall    = "fall";
        private const string StateDash    = "dash";
        private const string StateAim     = "aim";
        private const string StateShot    = "shot";
        private const string StateKick    = "kick";
        private const string StateGetHit  = "getHit";

        // ── Rutas de parámetros ───────────────────────────────────────
        private const string ParamPlayback    = "parameters/playback";
        private const string ParamKickRequest = "parameters/kick_shot/request";   // si usas OneShot
        private const string ParamKickActive  = "parameters/kick_shot/active";

        // ─────────────────────────────────────────────────────────────
        public override void _Ready()
        {
            _lira = (Lira)GetTree().GetFirstNodeInGroup("Lira");
            if (_lira == null)
            {
                GD.PrintErr("LiraAnimationTree: no se encontró Lira en el grupo 'Lira'.");
                return;
            }

            // Este script vive como script del nodo AnimationTree,
            // así que lo obtenemos con GetParent — pero solo si el padre ES un AnimationTree.
            if (GetParent() is AnimationTree tree)
            {
                _animTree = tree;
            }
            else
            {
                // Fallback: buscarlo por nombre desde Lira
                _animTree = _lira.GetNodeOrNull<AnimationTree>("AnimationTree");
            }

            if (_animTree == null)
            {
                GD.PrintErr("LiraAnimationTree: no se encontró el nodo AnimationTree.");
                return;
            }

            _animTree.Active = true;
            _stateMachine = (AnimationNodeStateMachinePlayback)
                _animTree.Get(ParamPlayback);

            if (_lira.animatedSprite != null)
                _lira.animatedSprite.AnimationFinished += OnAnimationFinished;
        }

        // ─────────────────────────────────────────────────────────────
        //  API pública — llamada desde los estados de movimiento
        // ─────────────────────────────────────────────────────────────

        /// <summary>Transiciona suavemente al estado indicado.</summary>
        public void Travel(string stateName)
        {
            if (_stateMachine == null) return;
            _stateMachine.Travel(stateName);
        }

        /// <summary>Salta directamente al estado sin blend (útil para interrupciones).</summary>
        public void Start(string stateName)
        {
            if (_stateMachine == null) return;
            _stateMachine.Start(stateName);
        }

        /// <summary>Devuelve el nombre del estado actual del StateMachine.</summary>
        public string CurrentState() => _stateMachine?.GetCurrentNode() ?? "";

        // ─────────────────────────────────────────────────────────────
        //  Métodos semánticos para cada estado de Lira
        // ─────────────────────────────────────────────────────────────

        public void PlayIdle()    => Travel(StateIdle);
        public void PlayRun()     => Travel(StateRun);
        public void PlayJump()    => Travel(StateJump);
        public void PlayFall()    => Travel(StateFall);
        public void PlayDash()    => Travel(StateDash);
        public void PlayAim()     => Travel(StateAim);
        public void PlayShot()    => Travel(StateShot);
        public void PlayGetHit()  => Start(StateGetHit);   // Interrupción directa

        /// <summary>
        /// Lanza la animación de kick.
        /// Si el StateMachine tiene un nodo OneShot llamado "kick_shot",
        /// lo dispara para superponerlo sobre el estado base.
        /// De lo contrario, viaja al estado "kick".
        /// </summary>
        public void PlayKick()
        {
            // Opción A — usando OneShot (recomendado si quieres superponer)
            // _animTree.Set(ParamKickRequest,
            //     (int)AnimationNodeOneShot.OneShotRequest.Fire);

            // Opción B — viaje directo al estado kick
            Start(StateKick);
        }

        /// <summary>¿Está activa la animación de kick (OneShot)?</summary>
        public bool IsKicking()
            => (bool)_animTree.Get(ParamKickActive);

        // ─────────────────────────────────────────────────────────────
        //  Sincronización automática con MovementStateMachine
        // ─────────────────────────────────────────────────────────────

        public override void _Process(double delta)
        {
        if (_lira == null || _stateMachine == null) return;
        
            string movState = _lira.MovementStateMachine?.CurrentStateName ?? "";
            
            if (movState == "DeathMovementState")
            {
                if (CurrentState() != "dead") Travel("dead");
                return; // Bloquea cualquier otra actualización (como el Idle)
            }

            switch (movState)
            {
                case "IdleMovementState":          PlayIdle();   break;
                case "RunningMovementState":        PlayRun();    break;
                case "JumpingMovementState":        PlayJump();   break;
                case "DoubleJumpMovementState":     PlayJump();   break;
                case "FallingMovementState":        PlayFall();   break;
                case "DashMovementState":           PlayDash();   break;
                case "MagicMovementState":          PlayAim();    break;
                case "AttackMovementState":
                    if (CurrentState() != StateKick)
                        {
                            _stateMachine.Start(StateKick); 
                        }
                break;
                case "KnockbackMovementState":      PlayGetHit(); break;
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Callbacks
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Cuando termina una animación de un solo ciclo (kick, shot, getHit),
        /// volvemos al estado base.
        /// </summary>
        private void OnAnimationFinished()
        {
            string anim = _lira?.animatedSprite?.Animation ?? "";
            switch (anim)
            {
                case "aim":
                    int lastFrame = _lira.animatedSprite.SpriteFrames
                        .GetFrameCount(_lira.animatedSprite.Animation) - 1;
                    _lira.animatedSprite.Frame = lastFrame;
                    _lira.animatedSprite.Pause();
                    break;
                case "MagicMovementState":
                    if (CurrentState() != "shot") 
                    {
                        PlayAim();
                    }
                break;
                case "getHit":
                    break;
                case "DeathMovementState":
                // Si aún no ha tocado el suelo, que siga en animación de caída
                // Una vez que el script de estado cambie a "die", el AnimTree lo mostrará
                if (CurrentState() != "dead" && !_lira.IsOnFloor())
                {
                    PlayFall();
                }
                else if (CurrentState() != "dead" && _lira.IsOnFloor())
                {
                    // Esto es un respaldo por si el Start("die") del estado tarda un frame
                    Start("dead");
                }
                break;
            }
        }

        /// <summary>
        /// Llama a este método cuando el jugador suelta el botón de aim.
        /// Reproduce aim al revés y luego vuelve a idle.
        /// </summary>
        public void AimReleased()
        {
            if (_lira?.animatedSprite == null) return;
            _lira.animatedSprite.Play();  // reactiva la reproducción antes del backwards
        }
    }
}
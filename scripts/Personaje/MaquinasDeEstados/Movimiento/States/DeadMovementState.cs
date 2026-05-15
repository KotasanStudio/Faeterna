using Godot;
using Faeterna.Scripts.Personaje.MaquinasDeEstados;
using PlayerType = Faeterna.Scripts.Personaje.Lira;

namespace Faeterna.scenes.Personajes
{
    public partial class DeadMovementState : State
    {
        private PlayerType _player;
        private bool _hasLanded;

        public override void Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
        }

        public override void Enter()
        {
			Engine.TimeScale = 0.8f;
            if (_player == null) return;

            _hasLanded = false;

            _player._deathScreen.ChangeVisibility(true);

            _player.CollisionLayer = 0;

            _player.AnimTree?.Travel("fall");
        }

        public override void Update(double delta)
        {
            if (_player == null || _hasLanded) return;

            // Aplicar gravedad manualmente para que caiga
            Vector2 velocity = _player.Velocity;
            velocity.Y += PlayerType.Gravity * (float)delta;
            _player.Velocity = velocity;
            _player.MoveAndSlide();

            // 4. Detectar el suelo
            if (_player.IsOnFloor())
            {
                _hasLanded = true;
                TriggerDeathOnGround();
            }
        }

        private void TriggerDeathOnGround()
        {
            _player.Velocity = Vector2.Zero;

		    // 1. Disparamos la muerte
		    _player.AnimTree?.Start("dead");

            // 2. IMPORTANTE: Desactivamos el proceso de actualización del AnimTree
            // Esto evita que el _Process de LiraAnimationTree siga enviando "PlayIdle()"
            if (_player.AnimTree != null) {
                _player.AnimTree.ProcessMode = ProcessModeEnum.Disabled;
            }

		    // 3. Forzamos al Sprite a reproducir la muerte manualmente ahora que el Tree no molesta
		    _player.animatedSprite.Play("dead");
		}

        public override void HandleInput(InputEvent ev) { /* Bloqueado */ }
    }
}

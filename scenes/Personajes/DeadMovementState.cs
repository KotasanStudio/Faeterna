using Godot;
using System;
using PlayerType = Faeterna.Scripts.Personaje.Lira;

namespace Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento.States
{
    public partial class DeadMovementState : State
    {
        private PlayerType _player;
        private bool _hasLanded = false;

        [Export] public string GameOverScenePath = "res://scenes/Menus/MainMenu.tscn";
        [Export] public float DelayAfterLanding = 2f;

        public override void Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
        }

        public override void Enter()
        {
			Engine.TimeScale = 0.8f;
            if (_player == null) return;

            _hasLanded = false;
            
            // 1. Bloqueo total de controles
            // Al entrar aquí, Update y HandleInput de otros estados ya no corren.
            
            // 2. Desactivar colisiones con enemigos (capa de daño)
            _player.CollisionLayer = 0;

            // 3. Animación de "caída herido" o simplemente dejar la de "fall"
            // Si tienes una animación de "tocado en el aire", ponla aquí.
            _player.AnimTree?.Travel("fall"); 
        }

        public override void Update(double delta)
        {
            if (_player == null || _hasLanded) return;

            // Aplicar gravedad manualmente para que caiga
            Vector2 velocity = _player.Velocity;
            velocity.Y += Lira.Gravity * (float)delta;
            _player.Velocity = velocity;
            _player.MoveAndSlide();

            // 4. Detectar el suelo
            if (_player.IsOnFloor())
            {
                _hasLanded = true;
                TriggerDeathOnGround();
            }
        }

        private async void TriggerDeathOnGround()
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

		    // 4. Espera antes de la pantalla de Game Over
		    await _player.ToSignal(_player.GetTree().CreateTimer(DelayAfterLanding), "timeout");

		    _player.GetTree().ChangeSceneToFile(GameOverScenePath);
		}

        public override void HandleInput(InputEvent ev) { /* Bloqueado */ }
    }
}
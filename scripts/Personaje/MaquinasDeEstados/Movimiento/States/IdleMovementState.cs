using Godot;
using System;
using Faeterna.Scripts.Personaje.MaquinasDeEstados;
using PlayerType = Faeterna.Scripts.Personaje.Lira;
using Faeterna.Scripts.Tools;
using Faeterna.scripts.Mapa;

namespace Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento.States
{
    public partial class IdleMovementState : State
    {
        private PlayerType _player;

        public override async void Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
        }

        public override void Enter()
        {
            if (_player == null) return;
            _player.PlayAudio("idle");
            _player.SetAnimation("idle");
            _player.MoveAndSlide();
        }

        public override void Update(double delta)
        {
            if (_player == null) return;
            if (!_player.IsOnFloor())
            {
                GD.Print("Transitioning to falling or jumping state from idle.");
                // En 2D, Y positivo = hacia abajo, negativo = hacia arriba (subiendo).
                stateMachine.TransitionTo("FallingMovementState");
            }
            else
            {
                if (Input.GetAxis("move_left", "move_right") != 0f)
                    stateMachine.TransitionTo("RunningMovementState");
            }
        }

        public override void HandleInput(InputEvent ev)
        {
            if (_player == null) return;
            if (ev.IsActionPressed("jump") && _player.IsOnFloor())
                stateMachine.TransitionTo("JumpingMovementState");
            if (ev.IsActionPressed("dash") && _player.HasDash())
                stateMachine.TransitionTo("DashMovementState");
            if (ev.IsActionPressed("aim"))
                stateMachine.TransitionTo("MagicMovementState");
            if (ev.IsActionPressed("kick"))
                stateMachine.TransitionTo("AttackMovementState");
            if (ev.IsActionPressed("interact")) 
            {
                var areas = _player.GetNode<Area2D>("HurtBox").GetOverlappingAreas();
        
                foreach (var area in areas)
                {
                    Node parent = area.GetParent();
        
                    // Verificamos si es algo interactuable
                    if (parent is CheckPoint || parent is Objeto)
                    {
                        var interactState = stateMachine.GetNode<StartPrayMovementState>("StartPrayMovementState");
                        // Configuramos los datos genéricos
                        interactState.InteractorNode = parent;
                        if (parent is CheckPoint cp) interactState.TargetPosition = cp.GetPrayPosition();
                        else if (parent is Objeto obj) interactState.TargetPosition = obj.GetPickUpPosition();
                        stateMachine.TransitionTo("StartPrayMovementState");
                        return;
                    }
                }
            }
        }
    }
}

using Godot;
using System;
using Faeterna.Scripts.Personaje.MaquinasDeEstados;
using PlayerType = Faeterna.Scripts.Personaje.Lira;
using Faeterna.Scripts.Tools;

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
            if (ev.IsActionPressed("dash"))
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
                    if (area.GetParent() is CheckPoint cp)
                    {
                        var prayState = stateMachine.GetNode<StartPrayMovementState>("StartPrayMovementState");
                        prayState.TargetPosition = cp.GetPrayPosition();
                        prayState.CurrentCheckPoint = cp;
                        stateMachine.TransitionTo("StartPrayMovementState");
                        return;
                    }
                }
            }
        }
    }
}

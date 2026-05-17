using System;
using Faeterna.Scripts.Enemigos;
using Faeterna.Scripts.Personaje;
using Godot;

namespace Faeterna.scripts.Enemigos.ReyJabali
{
  /// <summary>
  /// Boss Rey Jabalí. Gestiona sus patrones de ataque, persecución, dash,
  /// salto especial y la muerte con señal para desbloquear recompensas.
  /// </summary>
  public partial class ReyJabali : Enemy
  {
    /// <summary>
    /// Material del shader usado para el efecto visual de daño.
    /// </summary>
    private ShaderMaterial _shaderMaterial;

    /// <summary>
    /// Indica si el boss está actualmente dashing.
    /// </summary>
    private bool _isDashing;

    /// <summary>
    /// Indica si el boss está cargando un ataque.
    /// </summary>
    private bool _isChargingAttack;

    /// <summary>
    /// Indica si el boss ya murió.
    /// </summary>
    private bool _isDead;

    /// <summary>
    /// Estado auxiliar para detectar si el boss venía subiendo en el salto.
    /// </summary>
    private bool _jumped;

    /// <summary>
    /// Estado auxiliar para detectar si el boss ya entró en fase de caída.
    /// </summary>
    private bool _falled;

    /// <summary>
    /// Número de dashes realizados antes de entrar en la fase de salto especial.
    /// </summary>
    private int _dashCount;

    /// <summary>
    /// Número de saltos ejecutados dentro de la secuencia especial.
    /// </summary>
    private int _jumpCount;

    /// <summary>
    /// Cantidad de acciones antes de pasar al salto especial.
    /// </summary>
    private int _actionsBeforeJump = 3;

    /// <summary>
    /// Indica si el boss está volviendo a su posición inicial.
    /// </summary>
    private bool _isReturningHome;

    /// <summary>
    /// Velocidad de caminata normal del boss.
    /// </summary>
    public float WalkSpeed = 150f;

    /// <summary>
    /// Distancia mínima para considerar que el boss llegó a casa.
    /// </summary>
    private float _arrivalThreshold = 10f;

    /// <summary>
    /// Dirección horizontal actual del boss.
    /// </summary>
    private int _currentDirection;

    /// <summary>
    /// Posición inicial usada para volver a casa cuando el jugador se aleja.
    /// </summary>
    private Vector2 _initPosition;

    /// <summary>
    /// Indica si el boss empieza mirando hacia la izquierda.
    /// </summary>
    [Export] private bool _startFacingLeft;

    /// <summary>
    /// Área de detección del jugador.
    /// </summary>
    [ExportGroup("Collisions and Areas")]
    [Export] private Area2D _bossArea;

    /// <summary>
    /// Área de ataque del boss.
    /// </summary>
    [Export] private Area2D _attackHitBox;

    /// <summary>
    /// Área que recibe daño.
    /// </summary>
    [Export] private Area2D _hurtBox;

    /// <summary>
    /// Timer que controla la duración del dash.
    /// </summary>
    [ExportGroup("Timers")]
    [Export] private Timer _dashTimer;

    /// <summary>
    /// Timer que controla la carga del ataque.
    /// </summary>
    [Export] private Timer _loadAttackTimer;

    /// <summary>
    /// Timer que espera a que termine la animación de muerte.
    /// </summary>
    [Export] private Timer _deathAnimationTimer;

    /// <summary>
    /// Sonido del salto.
    /// </summary>
    [ExportGroup("Audio")]
    [Export] private AudioStream _jumpAudio;

    /// <summary>
    /// Sonido de daño recibido.
    /// </summary>
    [Export] private AudioStream _hitAudio;

    /// <summary>
    /// Sonido de carrera.
    /// </summary>
    [Export] private AudioStream _runAudio;

    /// <summary>
    /// Señal emitida cuando el boss muere.
    /// </summary>
    [Signal] public delegate void jabaliBossdeathEventHandler();

    /// <summary>
    /// Generador aleatorio usado para variar la secuencia de ataques.
    /// </summary>
    private Random _rnd = new();

    /// <summary>
    /// Captura la posición inicial al entrar en el árbol de escenas.
    /// </summary>
    public override void _EnterTree()
    {
      _initPosition = GlobalPosition;
    }

    /// <summary>
    /// Inicializa la dirección del boss, prepara el shader y fija la animación inicial.
    /// </summary>
    public override void _Ready()
    {

      if (_runAudio == null)
        _runAudio = GD.Load<AudioStream>("res://assets/Audio/Jabali/BoarRun.wav");

      if (_hitAudio == null)
        _hitAudio = GD.Load<AudioStream>("res://assets/Audio/Jabali/BoarJump.wav");

      if (_jumpAudio == null)
        _jumpAudio = GD.Load<AudioStream>("res://assets/Audio/Jabali/BoarJump.wav");


      if (_startFacingLeft)
      {
        _currentDirection = 1;
        FlipHJabali();
      }
      else
      {
        _currentDirection = -1;
      }

      _shaderMaterial = (ShaderMaterial)animatedSprite.Material;
      SetAnimation("idle");
    }

    /// <summary>
    /// Control principal de física del boss: gravedad, persecución, regreso a casa y knockback.
    /// </summary>
    /// <param name="delta">Tiempo transcurrido desde el último frame.</param>
    public override void _PhysicsProcess(double delta)
    {
      if (_isDead)
        return;

      Vector2 velocity = Velocity;

      if (!IsOnFloor())
      {
        velocity.Y += Gravity * (float)delta;
        if (velocity.Y < 0 && !_jumped)
        {
          SetAnimation("jump");
          playAudio(_jumpAudio);
          _jumped = true;
          _falled = false;
        }
        else if (velocity.Y > 0 && !_falled)
        {
          SetAnimation("fall");
          _falled = true;
          _jumped = false;
        }
      }
      else
      {
        // PRIORIDAD 1: Si el jugador está cerca, atacar.
        if (target != null)
        {
          _isReturningHome = false;
          if (_isChargingAttack)
          {
            velocity.X=0;
            _dashTimer.Stop();
            SetAnimation("loadAttack");
          }
          else if (_isDashing)
          {
            velocity.X = _currentDirection * dashSpeed;
            SetAnimation("run");
          }
          else if (!_isChargingAttack && !_isDashing)
          {
            bool shouldFlip = (target.GlobalPosition.X > GlobalPosition.X && _currentDirection < 0) ||
              (target.GlobalPosition.X < GlobalPosition.X && _currentDirection > 0);
            if (shouldFlip)
              FlipHJabali();

            _isChargingAttack = true;
            SetAnimation("idle");
            playAudio(null);
          }
        }
        // PRIORIDAD 2: Si el jugador se fue, volver a casa.
        else if (_isReturningHome)
        {
          float distanceToStart = _initPosition.X - GlobalPosition.X;

          if (Mathf.Abs(distanceToStart) > _arrivalThreshold)
          {
            int directionHome = distanceToStart > 0 ? 1 : -1;

            if (directionHome != _currentDirection)
              FlipHJabali();

            velocity.X = directionHome * WalkSpeed;
            SetAnimation("run");
          }
          else
          {
            velocity.X = 0;
            _isReturningHome = false;
            SetAnimation("idle");
            int initialDir = _startFacingLeft ? -1 : 1;
            if (_currentDirection != initialDir)
              FlipHJabali();
          }
        }
        // PRIORIDAD 3: Quieto.
        else
        {
          velocity.X = Mathf.MoveToward(Velocity.X, 0, dashSpeed * (float)delta);
          if (velocity.X == 0)
            SetAnimation("idle");
        }
      }

      Velocity = velocity;
      MoveAndSlide();
    }

    /// <summary>
    /// Se llama cuando termina el tiempo de carga del ataque y decide si el boss corre o salta.
    /// </summary>
    public void _on_load_attack_timer_timeout()
    {
      if (target == null || _isDead)
        return;

      _isChargingAttack = false;
      bool shouldFlip = (target.GlobalPosition.X > GlobalPosition.X && _currentDirection < 0) ||
                (target.GlobalPosition.X < GlobalPosition.X && _currentDirection > 0);
      if (shouldFlip)
        FlipHJabali();

      if (_dashCount >= _actionsBeforeJump)
      {
        Velocity = DoNextJump();

        GD.Print($"Salto {_jumpCount} ejecutado");

        if (_jumpCount < 3)
        {
          _loadAttackTimer.Start(0.8f);
        }
        else
        {
          _jumpCount = 0;
          _dashCount = 0;
          _actionsBeforeJump = _rnd.Next(2, 5);
          _isChargingAttack = true;
          _loadAttackTimer.Start(2f);
        }
      }
      else
      {
        _dashCount++;
        playAudio(_runAudio);
        _isDashing = true;
        _dashTimer.Start();
      }
    }

    /// <summary>
    /// Finaliza la fase de dash y prepara la siguiente carga del ataque.
    /// </summary>
    public void _on_dash_timer_timeout()
    {
      _isDashing = false;
      _isChargingAttack = true;

      if (target != null)
      {
        int directionToPlayer = target.GlobalPosition.X > GlobalPosition.X ? 1 : -1;
        if (directionToPlayer != _currentDirection)
          FlipHJabali();
      }

      _loadAttackTimer.Start();
    }

    /// <summary>
    /// Calcula el siguiente salto de la secuencia especial del boss.
    /// </summary>
    /// <returns>Vector de velocidad a aplicar al boss.</returns>
    private Vector2 DoNextJump()
    {
      if (_jumpCount >= 3)
      {
        _jumpCount = 0;
        _dashCount = 0;
        _actionsBeforeJump = _rnd.Next(1, 3);

        GD.Print("Ataque especial completado.");
        return new Vector2(0, Velocity.Y);
      }

      _jumpCount++;
      return new Vector2(_currentDirection * (dashSpeed / 2), jumpVelocity);
    }

    /// <summary>
    /// Voltea el sprite del boss y actualiza su dirección horizontal.
    /// </summary>
    private void FlipHJabali()
    {
      _currentDirection *= -1;
      animatedSprite.FlipH = _currentDirection < 0;
    }

    /// <summary>
    /// Aplica daño al jugador cuando entra en el hitbox de ataque del boss.
    /// </summary>
    /// <param name="prota">Nodo que entró en el hitbox.</param>
    public void _on_attack_hit_box_body_entered(Node2D prota)
    {
      if (prota is Lira lira)
        lira.TakeDamage(1, GlobalPosition);
    }

    /// <summary>
    /// Procesa el daño recibido por el boss desde su hurtbox.
    /// </summary>
    /// <param name="area">Área que colisionó con el boss.</param>
    private void OnHurtBoxAreaEntered(Area2D area)
    {
      if (_isDead)
        return;

      if (area.Name == "KickHitbox" && area.GetParent() is Lira lira)
      {
        TakeDamage(1, lira.GlobalPosition);
      }
      else if (area is Shot shot)
      {
        TakeDamage((int)(shot.Scale.X * 1.5f+1), shot.GlobalPosition);
      }

      if (target != null)
      {
        bool shouldFlip = (target.GlobalPosition.X > GlobalPosition.X && _currentDirection < 0) ||
                  (target.GlobalPosition.X < GlobalPosition.X && _currentDirection > 0);
        if (shouldFlip)
          FlipHJabali();
      }
    }

    /// <summary>
    /// Aplica daño al boss y controla la transición a muerte o al knockback.
    /// </summary>
    /// <param name="amount">Cantidad de daño recibida.</param>
    /// <param name="sourcePosition">Posición del atacante para calcular la dirección del knockback.</param>
    private void TakeDamage(int amount, Vector2 sourcePosition)
    {
      playAudio(_hitAudio);
      hitShader(_shaderMaterial);
      GD.Print(health);
      if(target==null) return;

      health -= amount;
      if (health > 0)
        return;

      _isDead = true;
      // Registrar boss derrotado para que persista en el guardado.
      Faeterna.Scripts.Tools.GameSaveService.MarkBossDefeated(this.GetType().Name);
      _isDashing = false;
      _isChargingAttack = false;
      Velocity = Vector2.Zero;

      _loadAttackTimer.Stop();

      DesactiveCollision();
      SetAnimation("die");
      EmitSignal(nameof(jabaliBossdeath));
      _deathAnimationTimer.Start();
    }

    /// <summary>
    /// Elimina el nodo del boss cuando termina la animación de muerte.
    /// </summary>
    private void OnDeathAnimationTimerTimeout() => QueueFree();

    /// <summary>
    /// Desactiva las colisiones del boss para que no interactúe tras morir.
    /// </summary>
    private void DesactiveCollision()
    {
      _attackHitBox.CollisionLayer = 0;
      _attackHitBox.CollisionMask = 0;

      _bossArea.CollisionLayer = 0;
      _bossArea.CollisionMask = 0;
    }

    /// <summary>
    /// Detecta la entrada del jugador en el área del boss y activa el combate.
    /// </summary>
    /// <param name="prota">Cuerpo que entró en el área del boss.</param>
    public void _on_boss_area_body_entered(Node2D prota)
    {
      GD.Print("Rey Jabali detecta algo entrando al área");
      if (prota is not Lira lira)
        return;

      target = lira;
      _isReturningHome = false;
      _isChargingAttack = true;
      _isDashing = false;

      int newDir = lira.GlobalPosition.X > GlobalPosition.X ? 1 : -1;
      if (newDir != _currentDirection)
        FlipHJabali();

      _loadAttackTimer.Start();
    }

    /// <summary>
    /// Detecta la salida del jugador y prepara el regreso del boss a su posición inicial.
    /// </summary>
    /// <param name="prota">Cuerpo que salió del área del boss.</param>
    public void _on_boss_area_body_exited(Node2D prota)
    {
      if (prota is Lira)
      {
        health = 30;
        target = null;
        _isDashing = false;
        _isChargingAttack = false;
        _loadAttackTimer.Stop();

        GetTree().CreateTimer(1.0f).Timeout += () =>
        {
          if (target == null)
            _isReturningHome = true;
        };
      }
    }

    /// <summary>
    /// Devuelve al boss a su posición inicial si el jugador se aleja.
    /// </summary>
    private void moveToinitPosition()
    {
      if (target != null || _isDead)
        return;

      GlobalPosition = _initPosition;
      Velocity = Vector2.Zero;

      int initialDir = _startFacingLeft ? -1 : 1;
      if (_currentDirection != initialDir)
        FlipHJabali();

      _isChargingAttack = false;
      _isDashing = false;
      SetAnimation("idle");
    }
  }
}

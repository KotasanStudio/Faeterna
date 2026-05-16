using Faeterna.Scripts.Personaje;
using Faeterna.Scripts.Enemigos;
using Godot;

namespace Faeterna.scripts.Enemigos.Wolf
{
  /// <summary>
  /// Lobo enemigo que alterna entre patrullar, hacer dash y responder a daño.
  /// </summary>
  public partial class Wolf : Enemy
  {
    /// <summary>Intervalo entre dashes automáticos.</summary>
    public float DashInterval = 2f;

    /// <summary>Timer que dispara el dash automático.</summary>
    private Timer _dashTimer;

    /// <summary>Indica si el lobo está actualmente en dash.</summary>
    private bool _isDashing;

    /// <summary>Audio de carrera del lobo.</summary>
    [ExportGroup("Audio")]
    [Export] private AudioStream _runAudio;

    /// <summary>Audio de impacto al recibir daño.</summary>
    [Export] private AudioStream _hitAudio;

    /// <summary>Audio de ataque usado en reacciones especiales.</summary>
    [Export] private AudioStream _attackAudio;

    /// <summary>
    /// Ajusta la orientación inicial del sprite y del área de detección antes de entrar en escena.
    /// </summary>
    public override void _EnterTree()
    {
      if (flipSprite)
      {
        animatedSprite.FlipH = flipSprite;
        detectionArea.Position = new Vector2(88.25f * dashDirection * -1, 0); // Ajusta el área de detección
      }
    }

    /// <summary>
    /// Inicializa el timer de dash, enlaza eventos y prepara la animación inicial.
    /// </summary>
    public override void _Ready()
    {
            if(_runAudio==null)
                _runAudio = GD.Load<AudioStream>("res://assets/Audio/Wolf/WolfRun.wav");

            if(_hitAudio==null)
                _hitAudio = GD.Load<AudioStream>("res://assets/Audio/Wolf/hit.mp3");

            if(_attackAudio==null)
                _attackAudio = GD.Load<AudioStream>("res://assets/Audio/Wolf/WolfAttack.wav");
    
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

    /// <summary>
    /// Lógica de física: gravedad, dash, knockback y movimiento principal.
    /// </summary>
    /// <param name="delta">Tiempo transcurrido desde el último frame.</param>
    public override void _PhysicsProcess(double delta)
    {
      Vector2 velocity = Velocity;

      if (!IsOnFloor())
        velocity.Y += Gravity * (float)delta;

      if (_isDashing)
      {
        // Durante el dash, mantenemos la velocidad constante en la dirección del dash.
        dashDuration -= (float)delta;

        // Fuerza la actualización del raycast en cada frame.
        groundCheck.ForceRaycastUpdate();

        // Para el dash si no hay suelo delante o se acabó el tiempo límite.
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
          playAudio(_attackAudio);
          SetAnimation("attack");

          velocity.X = 0f;
        }
      }

      if (health > 0)
      {
        Velocity = velocity;
        MoveAndSlide();
      }
    }

    /// <summary>
    /// Llamado cuando el timer de dash expira; decide nueva dirección y arranca el dash.
    /// </summary>
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
      playAudio(_runAudio);
    }

    /// <summary>
    /// Maneja el golpe del jugador sobre el hitbox de ataque del lobo.
    /// </summary>
    /// <param name="prota">Nodo que entró en el hitbox.</param>
    public void _on_attack_hit_box_body_entered(Node2D prota)
    {
      if (prota is Lira lira)
        lira.TakeDamage(1, GlobalPosition);
    }

    /// <summary>
    /// Procesa daño recibido por el lobo desde el hurtbox.
    /// </summary>
    /// <param name="area">Área que colisionó con el hurtbox.</param>
    private void OnHurtBoxAreaEntered(Area2D area)
    {
      if (area.Name == "KickHitbox" && area.GetParent() is Lira lira)
        TakeDamage(1, lira.GlobalPosition);
      if (area is Shot shot)
        TakeDamage((int)shot.Scale.X, shot.GlobalPosition);
    }

    /// <summary>
    /// Aplica daño al lobo, reproduce efectos y gestiona la muerte o el knockback.
    /// </summary>
    /// <param name="v">Cantidad de daño a aplicar.</param>
    /// <param name="globalPosition">Posición del atacante para calcular el knockback.</param>
    private void TakeDamage(int v, Vector2 globalPosition)
    {
      health -= v;
      hitShader(shaderMaterial);
      playAudio(_hitAudio);
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

        // Dirección opuesta al atacante + mini salto.
        float directionX = GlobalPosition.X >= globalPosition.X ? 1.0f : -1.0f;
        Velocity = new Vector2(directionX * 250f, -200f);
      }
    }

    /// <summary>
    /// Registra al jugador como objetivo cuando entra en el área de detección.
    /// </summary>
    /// <param name="prota">Cuerpo que entró en el área.</param>
    public void _on_detection_area_body_entered(Node2D prota)
    {
      if (prota is Lira lira)
      {
        target = lira;
      }
    }

    /// <summary>
    /// Limpia el objetivo cuando el jugador sale del área de detección.
    /// </summary>
    /// <param name="prota">Cuerpo que salió del área.</param>
    public void _on_detection_area_body_exited(Node2D prota)
    {
      if (prota is Lira)
        target = null;
    }
  }
}

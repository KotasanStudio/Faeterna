using Godot;
using System;

namespace Faeterna.Scripts.Enemigos
{
    /// <summary>
    /// Clase base para todos los enemigos del juego. Proporciona funcionalidades comunes como
    /// gestión de salud, animaciones, efectos de sonido y física (gravedad, knockback, etc.).
    /// Esta clase hereda de CharacterBody2D y es la base para enemigos específicos como
    /// Jabali, Slime, Wolf y ReyJabali.
    /// </summary>
    public partial class Enemy : CharacterBody2D
    {
        /// <summary>Puntos de vida actuales del enemigo. Se asigna desde el editor.</summary>
        [Export] public int health;

        /// <summary>Velocidad de desplazamiento durante el dash del enemigo (px/s). Se asigna desde el editor.</summary>
        [Export] public float dashSpeed;

        /// <summary>Velocidad vertical aplicada al iniciar un salto (negativa = hacia arriba). Se asigna desde el editor.</summary>
        [Export] public float jumpVelocity;

        /// <summary>Referencia al nodo AnimatedSprite2D que renderiza las animaciones del enemigo. Se asigna desde el editor.</summary>
        [Export] public AnimatedSprite2D animatedSprite;

        /// <summary>Duración del dash del enemigo en segundos. Se asigna desde el editor.</summary>
        [Export] public float dashDuration;

        /// <summary>Duración del knockback aplicado al enemigo en segundos. Se asigna desde el editor.</summary>
        [Export]  public float knockbackDuration;

        /// <summary>Booleano que controla si el sprite del enemigo debe estar invertido (mirada izquierda/derecha). Se asigna desde el editor.</summary>
        [Export] public bool flipSprite;

        /// <summary>Reproductor de audio para los efectos de sonido del enemigo. Se asigna desde el editor.</summary>
        [Export] public AudioStreamPlayer2D audioPlayer;

        [ExportGroup("Collisions and Areas")]
        /// <summary>Área de detección del enemigo utilizada para detectar al jugador. Se asigna desde el editor.</summary>
        [Export] public CollisionShape2D detectionArea;

        /// <summary>RayCast2D que verifica si el enemigo está en el suelo. Se asigna desde el editor.</summary>
        [Export] public RayCast2D groundCheck;

        /// <summary>Área de daño del enemigo que causa daño al jugador al colisionar. Se asigna desde el editor.</summary>
        [Export] public Area2D hurtBox;

        /// <summary>Área de ataque del enemigo que inflige daño cuando golpea. Se asigna desde el editor.</summary>
        [Export] public Area2D attackHitBox;

        /// <summary>Dirección del dash del enemigo (1 = derecha, -1 = izquierda).</summary>
        public int dashDirection = 1;

        /// <summary>Temporizador para controlar la duración del knockback aplicado al enemigo.</summary>
        public float knockbackTimer = 0f;

        /// <summary>Material de shader utilizado para efectos visuales de daño (como destello blanco).</summary>
        public ShaderMaterial shaderMaterial;

        /// <summary>Referencia al objetivo actual del enemigo (generalmente el jugador). Es null si no hay objetivo.</summary>
        public Node2D target = null;

        /// <summary>Gravedad obtenida del ProjectSettings, utilizada para la física del enemigo.</summary>
        public static float Gravity => ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

        /// <summary>
        /// Inicialización del nodo. Obtiene la referencia al AnimatedSprite2D hijo si no está asignado.
        /// </summary>
        public override void _Ready()
        {
            if (animatedSprite == null)
                animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        }

        /// <summary>
        /// Aplica un efecto de shader de destello blanco al enemigo cuando recibe daño.
        /// El efecto dura 0.1 segundos (100 ms) y luego vuelve al estado normal.
        /// </summary>
        /// <param name="shader">Material de shader al cual se le aplicará el efecto de destello.</param>
        public async void hitShader(ShaderMaterial shader)
        {
            shader.SetShaderParameter("white_amount", 1.0f);

            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

            shader.SetShaderParameter("white_amount", 0.0f);
        }

        /// <summary>
        /// Reproduce un efecto de sonido del enemigo a través del reproductor de audio.
        /// </summary>
        /// <param name="audio">AudioStream que contiene el sonido a reproducir.</param>
        public void playAudio(AudioStream audio)
        {
            audioPlayer.Stream = audio;
            audioPlayer.Play();
        }

        /// <summary>
        /// Reproduce una animación en el sprite del enemigo.
        /// </summary>
        /// <param name="animationName">Nombre de la animación a reproducir.</param>
        public void SetAnimation(string animationName)
        {
            animatedSprite?.Play(animationName);
        }
    }
}

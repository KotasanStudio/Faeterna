using Godot;
using Godot.Collections;

namespace Faeterna.scripts.Player
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

        /// <summary>Indica si el doble salto está disponible.</summary>
        public bool DoubleJumpAvailable = true;

        /// <summary>Indica si el dash está disponible.</summary>
        public bool DashAvailable = true;

        /// <summary>Indica si el coyote time está disponible (permite saltar brevemente tras abandonar el suelo).</summary>
        public bool CoyoteAvailable = true;

        /// <summary>Referencia al nodo <see cref="AnimatedSprite2D"/> hijo que muestra las animaciones del personaje.</summary>
        public AnimatedSprite2D animatedSprite;

        /// <summary>Lista de <see cref="TextureRect"/> que representan los corazones de vida en la interfaz.</summary>
        [Export] Array<TextureRect> _hearts;

        /// <summary><see cref="TextureRect"/> que representa la barra de maná en la interfaz.</summary>
        [Export] TextureRect _manaBar;

        /// <summary>
        /// Inicialización del nodo. Obtiene la referencia al <see cref="AnimatedSprite2D"/> hijo.
        /// </summary>
        public override void _Ready()
        {
            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
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
        public async void TakeDamage(int amount)
        {
            GD.Print($"Duele");
            for (int i = 0; i < amount; i++)
            {
                if (_currentHealth <= 0)
                    return;

                _currentHealth--;

                TextureRect heart = _hearts[_currentHealth];

                if (heart.Material is ShaderMaterial mat)
                {
                    mat.SetShaderParameter("damage_flash", 1.0f);

                    await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

                    mat.SetShaderParameter("damage_flash", 0.0f);

                    mat.SetShaderParameter("fill_amount", 0.0f);
                }
            }
        }

        /// <summary>
        /// Actualiza visualmente los corazones de la interfaz según la salud actual.
        /// </summary>
        private void UpdateHearts()
        {
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

        public void VisibleUI(bool visible)
        {
            foreach (TextureRect heart in _hearts)
            {
                heart.Visible = visible;
            }

            _manaBar.Visible = visible;
        }
    }
}

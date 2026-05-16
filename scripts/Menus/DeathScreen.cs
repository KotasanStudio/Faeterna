using Godot;
using Faeterna.Scripts.Tools;
using System.Threading.Tasks;

namespace Faeterna.Scripts.Menus
{
    /// <summary>
    /// Pantalla de muerte del juego. Permite continuar desde el último guardado o
    /// volver al menú principal. También gestiona un pequeño fade-in al mostrarse.
    /// </summary>
    public partial class DeathScreen : Control
    {
        /// <summary>Ruta por defecto a la escena principal del juego si no existe una escena guardada válida.</summary>
        private const string DefaultGameScenePath = "res://scenes/Maps/Bosque.tscn";

        /// <summary>Duración del efecto de aparición gradual de la pantalla de muerte.</summary>
        private const float FadeInDuration = 0.65f;

        /// <summary>Botón que continúa la partida cargando el último guardado.</summary>
        [Export] private TextureButton _continueButton;

        /// <summary>Botón que devuelve al jugador al menú principal.</summary>
        [Export] private TextureButton _returnButton;

        /// <summary>
        /// Inicializa la pantalla de muerte. La deja oculta por defecto y configura
        /// los botones para que funcionen incluso durante la pausa del juego.
        /// </summary>
        public override void _Ready()
        {
            Visible = false;
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0f);

            // Configurar para que procese durante la pausa
            ProcessMode = ProcessModeEnum.Always;

            if (_continueButton != null)
            {
                _continueButton.Pressed += OnContinuarPressed;
                _continueButton.ProcessMode = ProcessModeEnum.Always;
            }
            if (_returnButton != null)
            {
                _returnButton.Pressed += OnReturnPressed;
                _returnButton.ProcessMode = ProcessModeEnum.Always;
            }
        }

        /// <summary>
        /// Reanuda la partida desde el último guardado del slot activo.
        /// Reproduce la animación del botón y luego cambia a la escena guardada.
        /// </summary>
        public void OnContinuarPressed()
        {
            ButtonTools.PlayPressAnimation(
                _continueButton,
                () => _ = ContinueFromLastSaveAsync()
            );

            Engine.TimeScale = 1f;
        }

        /// <summary>
        /// Carga la partida guardada más reciente del slot activo y vuelve a la escena asociada.
        /// Si no existe una escena guardada válida, usa la escena principal por defecto.
        /// </summary>
        private async Task ContinueFromLastSaveAsync()
        {
            GameData gameData = await GameSaveService.LoadActiveSlotAsync();
            string scenePath = !string.IsNullOrWhiteSpace(gameData?.ScenePath)
                ? gameData.ScenePath
                : DefaultGameScenePath;

            GameSaveService.RequestLoadActiveSlotOnNextScene();
            GetTree().ChangeSceneToFile(scenePath);
            Engine.TimeScale = 1f;
        }

        /// <summary>
        /// Devuelve al jugador al menú principal.
        /// </summary>
        public void OnReturnPressed()
        {
            ButtonTools.PlayPressAnimation(
                _returnButton,
                () =>
                {
                    GetTree().ChangeSceneToFile("res://scenes/Menus/MainMenu.tscn");
                }
            );
        }

        /// <summary>
        /// Muestra la pantalla de muerte con un fade-in suave.
        /// </summary>
        /// <param name="visible">Indica si la pantalla debe mostrarse. Solo actúa cuando es <see langword="true"/>.</param>
        public void ChangeVisibility(bool visible)
        {
            if (!visible)
                return;

            Visible = true;
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0f);

            Tween fadeTween = CreateTween();
            fadeTween.TweenProperty(this, "modulate:a", 1f, FadeInDuration)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out);
        }
    }
}

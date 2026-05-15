using Godot;
using Faeterna.Scripts.Tools;
using System.Threading.Tasks;

namespace Faeterna.Scripts.Menus
{
    public partial class DeathScreen : Control
    {
        private const string DefaultGameScenePath = "res://scenes/Maps/Bosque.tscn";
        private const float FadeInDuration = 0.65f;

        [Export] private TextureButton _continueButton;
        [Export] private TextureButton _returnButton;

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

        public void OnContinuarPressed()
        {
            ButtonTools.PlayPressAnimation(
                _continueButton,
                () => _ = ContinueFromLastSaveAsync()
            );

            Engine.TimeScale = 1f;
        }

        private async Task ContinueFromLastSaveAsync()
        {
            GameData gameData = await GameSaveService.LoadActiveSlotAsync();
            string scenePath = !string.IsNullOrWhiteSpace(gameData?.ScenePath)
                ? gameData.ScenePath
                : DefaultGameScenePath;

            GetTree().ChangeSceneToFile(scenePath);
            Engine.TimeScale = 1f;
        }

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

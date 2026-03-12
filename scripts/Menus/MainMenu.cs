using Faeterna.scripts.Tools;
using Godot;

namespace Faeterna.scripts.Menus
{
    public partial class MainMenu : Control
    {
        [Export]
        private TextureButton _startButton;

        [Export]
        private TextureButton _settingsButton;

        [Export]
        private TextureButton _exitButton;

        [Export]
        private OptionsMenu _optionsMenu;

        [Export]
        private PlayMenu _playMenu;

        [Export]
        private TextureRect _background;

        public override void _Ready()
        {
            GetNode<TextureRect>("TextureRect").MouseFilter = MouseFilterEnum.Ignore;
            _startButton.Pressed += OnPlayPressed;
            _settingsButton.Pressed += OnSettingsPressed;
            _exitButton.Pressed += OnExitPressed;
        }

        public override void _Process(double delta)
        {
            if (!_optionsMenu.Visible && !_playMenu.Visible)
            {
                _startButton.Visible = true;
                _settingsButton.Visible = true;
                _exitButton.Visible = true;
                SetBlur(0f);
            }
        }

        private void OnPlayPressed()
        {
            ButtonTools.PlayPressAnimation(
                _startButton,
                () =>
                {
                    GD.Print("Play button pressed. Transitioning to scene...");
                    _startButton.Visible = false;
                    _settingsButton.Visible = false;
                    _exitButton.Visible = false;
                    SetBlur(2.45f);
                    _playMenu.Visible = true;
                }
            );
        }

        private void OnSettingsPressed()
        {
            ButtonTools.PlayPressAnimation(
                _settingsButton,
                () =>
                {
                    GD.Print("Settings button pressed. Transitioning to scene...");
                    _startButton.Visible = false;
                    _settingsButton.Visible = false;
                    _exitButton.Visible = false;
                    SetBlur(2.45f);
                    _optionsMenu.Visible = true;
                }
            );
        }

        private void OnExitPressed()
        {
            ButtonTools.PlayPressAnimation(
                _exitButton,
                () =>
                {
                    GD.Print("Exit button pressed. Quitting game...");
                    GetTree().Quit();
                }
            );
        }

        private void SetBlur(float intensidad)
        {
            if (_background?.Material is ShaderMaterial shader)
            {
                shader.SetShaderParameter("intensidad", intensidad);
            }
        }
    }
}

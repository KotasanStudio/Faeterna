using Faeterna.scripts.Player;
using Faeterna.scripts.Tools;
using Godot;

namespace Faeterna.scripts.Menus
{
	public partial class PauseMenu : Control
	{
		[Export] private TextureButton _returnButton;
        [Export] private TextureButton _settingsButton;
        [Export] private TextureButton _exitButton;
        [Export] private Control _optionsMenu;
        [Export] private Lira _player;

		public override void _Ready()
		{
			// El menú y sus controles deben procesar durante pausa.
			ApplyAlwaysProcessRecursive(this);

			var overlay = GetNodeOrNull<Control>("ColorRect");
			if (overlay != null)
			{
				// El fondo oscuro no debe bloquear clicks sobre los botones.
				overlay.MouseFilter = MouseFilterEnum.Ignore;
			}

			if (_returnButton != null)
			{
				_returnButton.Pressed += ResumeGame;
			}

			if (_settingsButton != null)
			{
				_settingsButton.Pressed += OpenSettings;
			}

			if (_exitButton != null)
			{
				_exitButton.Pressed += ExitToMenu;
			}

			if (_optionsMenu != null)
			{
				_optionsMenu.Visible = false;
				_optionsMenu.ProcessMode = ProcessModeEnum.Always;
			}

			Visible = false;
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (!IsPauseInput(@event))
			{
				return;
			}

			if (GetTree().Paused && _optionsMenu != null && _optionsMenu.Visible)
			{
				_optionsMenu.Visible = false;
			}
			else
			{
				TogglePause();
			}

			GetViewport().SetInputAsHandled();
		}

		private static bool IsPauseInput(InputEvent @event)
		{
			if (@event.IsActionPressed("pause") || @event.IsActionPressed("ui_cancel"))
			{
				return true;
			}

			if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
			{
				return keyEvent.Keycode == Key.Escape;
			}

			return false;
		}

		private static void ApplyAlwaysProcessRecursive(Node node)
		{
			node.ProcessMode = ProcessModeEnum.Always;
			foreach (Node child in node.GetChildren())
			{
				ApplyAlwaysProcessRecursive(child);
			}
		}

		private void TogglePause()
		{
			if (GetTree().Paused)
			{
				ResumeGame();
				return;
			}

			PauseGame();
		}

		private void PauseGame()
		{
			Visible = true;
			GetTree().Paused = true;
			Input.MouseMode = Input.MouseModeEnum.Visible;
            _player.VisibleUI(false);
		}

		public void ResumeGame()
        {
            ButtonTools.PlayPressAnimation(
                _returnButton,
                () =>
                {
                    if (_optionsMenu != null)
                    {
                        _optionsMenu.Visible = false;
                    }

                    GetTree().Paused = false;
                    Visible = false;
                    _player.VisibleUI(true);
                });
        }

		private void OpenSettings()
        {
            ButtonTools.PlayPressAnimation(
                _settingsButton,
                () =>
                {
                    _optionsMenu.Visible = true;
                });
        }

		private void ExitToMenu()
        {
            ButtonTools.PlayPressAnimation(
                _exitButton,
                () =>
                {
                    GetTree().Paused = false;
                    GetTree().ChangeSceneToFile("res://scenes/Menus/MainMenu.tscn");
                });
        }

		public override void _ExitTree()
		{
			// Evita dejar el juego pausado si esta escena se libera.
			if (GetTree() != null && GetTree().Paused)
			{
				GetTree().Paused = false;
			}
		}
	}
}

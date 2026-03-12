using Faeterna.scripts.Tools;
using Godot;
using Godot.Collections;

namespace Faeterna.scripts.Menus
{
    public partial class PlayMenu : Control
    {
        [Export]
        private Array<TextureButton> _saveButtons;

        [Export]
        private TextureButton _deleteButton;

        [Export]
        private TextureButton _backButton;

        public override void _Ready() { }

        private void OnSavePressed(int id)
        {
            // Reproducir la animación de press en el botón correspondiente
            if (id >= 0 && id < _saveButtons.Count)
            {
                ButtonTools.PlayPressAnimation(
                    _saveButtons[id],
                    () =>
                    {
                        GD.Print(
                            $"Save button {id} pressed. Transitioning to game scene with save slot {id}..."
                        );
                        GetTree().ChangeSceneToFile("res://scenes/Maps/DevRoom.tscn");
                    }
                );
            }
        }

        private void OnDeletePressed()
        {
            ButtonTools.PlayPressAnimation(
                _deleteButton,
                () =>
                {
                    GD.Print("Delete button pressed.");
                }
            );
        }

        private void OnBackPressed()
        {
            ButtonTools.PlayPressAnimation(
                _backButton,
                () =>
                {
                    this.Visible = false;
                }
            );
        }
    }
}

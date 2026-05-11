using Faeterna.scripts.Tools;
using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace Faeterna.scripts.Menus
{
    public partial class PlayMenu : Control
    {
        private const string DefaultGameScenePath = "res://scenes/Maps/Bosque.tscn";
        private const string ContinueTexturePath = "res://assets/Menu/SaveBottonContinue.png";

        [Export]
        private Array<TextureButton> _saveButtons;

        [Export]
        private TextureButton _deleteButton;

        [Export]
        private TextureButton _backButton;

        [Export]
        private Texture2D _continueTexture;

        private int _selectedSlot;
        private readonly List<Texture2D> _defaultSlotTextures = new();

        public override void _Ready()
        {
            _selectedSlot = 0;

            // Fallback por si la escena no tiene asignado el recurso exportado.
            if (_continueTexture == null)
            {
                _continueTexture = GD.Load<Texture2D>(ContinueTexturePath);
            }

            CacheDefaultSlotTextures();
            RefreshSlotVisuals();
            VisibilityChanged += () =>
            {
                if (Visible)
                {
                    RefreshSlotVisuals();
                }
            };
        }

        private void OnSavePressed(int id)
        {
            // Reproducir la animación de press en el botón correspondiente
            if (id >= 0 && id < _saveButtons.Count)
            {
                _selectedSlot = id;
                ButtonTools.PlayPressAnimation(
                    _saveButtons[id],
                    () =>
                    {
                        GameSaveService.SetActiveSlot(id);
                        GD.Print(
                            $"Save button {id} pressed. Transitioning to game scene with save slot {id}..."
                        );
                        if (id == 1)
                            GetTree().ChangeSceneToFile("res://scenes/Maps/Bosque.tscn");
                        else
                            GetTree().ChangeSceneToFile(DefaultGameScenePath);
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
                    GameSaveService.DeleteSlot(_selectedSlot);
                    GD.Print($"Delete button pressed. Slot {_selectedSlot + 1} eliminado.");
                    RefreshSlotVisuals();
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

        private void CacheDefaultSlotTextures()
        {
            _defaultSlotTextures.Clear();

            foreach (TextureButton button in _saveButtons)
            {
                _defaultSlotTextures.Add(button.TextureNormal);
            }
        }

        private void RefreshSlotVisuals()
        {
            for (int i = 0; i < _saveButtons.Count; i++)
            {
                TextureButton slotButton = _saveButtons[i];
                bool hasSave = GameSaveService.HasSave(i);

                slotButton.TextureNormal = hasSave && _continueTexture != null
                        ? _continueTexture
                        : _defaultSlotTextures[i];

                Label label = slotButton.GetNodeOrNull<Label>("Label");
                if (label != null)
                {
                    label.Visible = true;
                    label.Text = $"Save {i + 1}";
                }
            }
        }
    }
}

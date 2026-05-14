using Faeterna.Scripts.Tools;
using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace Faeterna.Scripts.Menus
{
    /// <summary>
    /// Control que representa el menú de selección de partidas guardadas del juego. Permite al jugador seleccionar un espacio de guardado,
    /// ver información sobre partidas guardadas, y cargar o eliminar partidas. El menú muestra botones para cada espacio de guardado (slot)
    /// y permite navegar entre ellos. Al seleccionar una partida guardada, se carga la escena correspondiente.
    /// Si una partida ha sido guardada previamente, muestra una textura especial "Continue" para indicarlo.
    /// </summary>
    public partial class PlayMenu : Control
    {
        private const string DefaultGameScenePath = "res://scenes/Maps/Bosque.tscn";
        private const string ContinueTexturePath = "res://assets/Menu/SaveBottonContinue.png";

        /// <summary>
        /// Array de botones que representan los espacios de guardado disponibles (slots). Se asignan desde el editor a través de export variable.
        /// Los botones permiten seleccionar un espacio específico para cargar o guardar una partida. Es importante que estas referencias estén correctamente asignadas
        /// para que el menú de selección de partidas funcione correctamente.
        /// </summary>
        [Export]
        private Array<TextureButton> _saveButtons;

        /// <summary>
        /// Botón para eliminar la partida guardada en el espacio seleccionado. Al hacer clic, borra los datos guardados del slot actual.
        /// Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el botón de eliminar
        /// funcione correctamente en el menú de selección de partidas.
        /// </summary>
        [Export]
        private TextureButton _deleteButton;

        /// <summary>
        /// Botón para volver desde el menú de selección de partidas al menú principal. Al hacer clic, oculta este menú y vuelve a mostrar el menú anterior.
        /// Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el botón de retroceso
        /// funcione correctamente en el menú de selección de partidas.
        /// </summary>
        [Export]
        private TextureButton _backButton;

        /// <summary>
        /// Textura que se muestra en los botones de guardado cuando existe una partida guardada en ese espacio, indicando que se puede continuar desde esa partida.
        /// Se asigna desde el editor a través de export variable. Si no está asignada, se carga desde la ruta predeterminada. Es importante que esta referencia esté
        /// correctamente asignada para que las visuales de partidas guardadas funcionen correctamente en el menú de selección.
        /// </summary>
        [Export]
        private Texture2D _continueTexture;

        /// <summary>Índice del espacio de guardado actualmente seleccionado el menú de selección de partidas.</summary>
        private int _selectedSlot;

        /// <summary>Indica si el menú está esperando confirmación para borrar una partida.</summary>
        private bool _deleteMode;

        /// <summary>Lista que cache las texturas predeterminadas de cada botón de espacio de guardado, para poder restaurarlas cuando se elimina una partida guardada.</summary>
        private readonly List<Texture2D> _defaultSlotTextures = new();

        /// <summary>
        /// Se llama cuando el nodo entra en la escena. Carga la textura de "Continue" si no está asignada desde el editor,
        /// guarda las texturas predeterminadas de todos los botones de slots, actualiza las visuales de los espacios de guardado,
        /// y configura los eventos de visibilidad para refrescar las visuales cada vez que el menú se abre.
        /// </summary>
        public override void _Ready()
        {
            _selectedSlot = -1;
            _deleteMode = false;

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

        /// <summary>
        /// Se llama cuando se presiona un botón de espacio de guardado. Si NO estamos en modo delete, carga la partida.
        /// Si estamos en modo delete, marca el slot con borde rojo para ser eliminado.
        /// </summary>
        /// <param name="id">Índice del botón de slot que fue presionado.</param>
        private void OnSavePressed(int id)
        {
            Engine.TimeScale = 1f;
            if (id < 0 || id >= _saveButtons.Count)
            {
                return;
            }

            // Reproducir la animación de press en el botón correspondiente
            ButtonTools.PlayPressAnimation(
                _saveButtons[id],
                () =>
                {
                // Si estamos en modo delete, marcar el slot para eliminar
                if (_deleteMode)
                {
                    // Si había otro slot seleccionado, restaurar su borde
                    if (_selectedSlot >= 0 && _selectedSlot != id)
                    {
                        UpdateSlotBorder(_saveButtons[_selectedSlot], false);
                    }

                    _selectedSlot = id;
                    UpdateSlotBorder(_saveButtons[id], true);
                    GD.Print($"Slot {id + 1} seleccionado para eliminar. Presiona Delete de nuevo para confirmar.");
                }
                // Si NO estamos en modo delete, cargar la partida
                else
                {
                    _selectedSlot = id;
                    GameSaveService.SetActiveSlot(id);
                    GD.Print($"Save button {id} pressed. Transitioning to game scene with save slot {id}...");
                    if (id == 1)
                        GetTree().ChangeSceneToFile("res://scenes/Maps/Bosque.tscn");
                    else
                        GetTree().ChangeSceneToFile(DefaultGameScenePath);
                }
                }
            );
        }

        /// <summary>
        /// Se llama cuando se presiona el botón de eliminar. Primer clic activa el modo delete para que selecciones un slot.
        /// Segundo clic (después de seleccionar un slot) elimina la partida guardada.
        /// </summary>
        private void OnDeletePressed()
        {
            // Si NO estamos en modo delete, activarlo con animación y limpiar cualquier selección previa.
            if (!_deleteMode)
            {
                ButtonTools.PlayPressAnimation(
                    _deleteButton,
                    () =>
                    {
                        _deleteMode = true;
                        if (_selectedSlot >= 0 && _selectedSlot < _saveButtons.Count)
                        {
                            UpdateSlotBorder(_saveButtons[_selectedSlot], false);
                        }

                        _selectedSlot = -1;
                        GD.Print("Modo delete activado. Selecciona un slot para eliminar.");
                    }
                );
                return;
            }

            if (_selectedSlot < 0 || _selectedSlot >= _saveButtons.Count)
            {
                ButtonTools.PlayPressAnimation(
                    _deleteButton,
                    () => GD.Print("Modo delete activo, pero no hay slot seleccionado para borrar.")
                );
                return;
            }

            // Si estamos en modo delete y hay un slot seleccionado, eliminarlo
            ButtonTools.PlayPressAnimation(
                _deleteButton,
                () =>
                {
                    GameSaveService.DeleteSlot(_selectedSlot);
                    GD.Print($"Delete button pressed. Slot {_selectedSlot + 1} eliminado.");

                    // Resetear visuales
                    UpdateSlotBorder(_saveButtons[_selectedSlot], false);
                    _deleteMode = false;
                    _selectedSlot = -1;

                    RefreshSlotVisuals();
                }
            );
        }

        /// <summary>
        /// Se llama cuando se presiona el botón de retroceso. Reproduce la animación de presión y luego oculta este menú,
        /// volviendo al menú anterior (generalmente el menú principal).
        /// </summary>
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

        /// <summary>
        /// Actualiza el color del botón de slot para indicar si está seleccionado para eliminación.
        /// Si está seleccionado, el borde se tintará de rojo; si no, es blanco (normal).
        /// </summary>
        /// <param name="button">Botón de slot a actualizar.</param>
        /// <param name="isSelected">Si true, se tinta de rojo; si false, de blanco.</param>
        private void UpdateSlotBorder(TextureButton button, bool isSelected)
        {
            button.Modulate = isSelected ? Colors.Red : Colors.White;
        }

        /// <summary>
        /// Guarda las texturas predeterminadas de cada botón de espacio de guardado en una lista cache.
        /// Estas texturas se restauran cuando se elimina una partida guardada para mostrar nuevamente el estado "vacío" del slot.
        /// </summary>
        private void CacheDefaultSlotTextures()
        {
            _defaultSlotTextures.Clear();

            foreach (TextureButton button in _saveButtons)
            {
                _defaultSlotTextures.Add(button.TextureNormal);
            }
        }

        /// <summary>
        /// Actualiza las visuales de todos los espacios de guardado. Si existe una partida guardada en un slot,
        /// muestra la textura de "Continue"; si no, muestra la textura predeterminada del slot.
        /// También actualiza el texto de etiqueta de cada botón para mostrar el número del slot y resetea los bordes a color normal.
        /// </summary>
        private void RefreshSlotVisuals()
        {
            for (int i = 0; i < _saveButtons.Count; i++)
            {
                TextureButton slotButton = _saveButtons[i];
                bool hasSave = GameSaveService.HasSave(i);

                slotButton.TextureNormal = hasSave && _continueTexture != null
                        ? _continueTexture
                        : _defaultSlotTextures[i];

                // Resetear color
                slotButton.Modulate = Colors.White;

                Label label = slotButton.GetNodeOrNull<Label>("Label");
                if (label != null)
                {
                    label.Visible = true;
                    label.Text = $"Save {i + 1}";
                }
            }

            _deleteMode = false;
            _selectedSlot = -1;
        }
    }
}

using Faeterna.scripts.Player;
using Faeterna.scripts.Tools;
using Godot;

namespace Faeterna.scripts.Menus
{
    /// <summary>
    /// Control que representa el menú de pausa del juego. Permite pausar y reanudar el juego, acceder a opciones y salir al menú principal.
    /// </summary>
	public partial class PauseMenu : Control
	{
        /// <summary>Botones y controles del menú de pausa. Se asignan desde el editor a través de export variables.</summary>
		[Export] private TextureButton _returnButton;

        /// <summary>Botón para abrir el menú de opciones. Al hacer clic, muestra el menú de opciones dentro del menú de pausa.</summary>
        [Export] private TextureButton _settingsButton;

        /// <summary>Botón para salir al menú principal. Al hacer clic, detiene la pausa y cambia la escena a la del menú principal.</summary>
        [Export] private TextureButton _exitButton;

        /// <summary>Contenedor de controles para las opciones del juego (ajustes de audio, video, controles, etc.). Se muestra u oculta al interactuar con el botón de ajustes. Procesa durante la pausa para permitir interacción con sus controles incluso cuando el juego está pausado.</summary>
        [Export] private Control _optionsMenu;

        /// <summary>Referencia al jugador (Lira) para controlar la visibilidad de su interfaz de usuario (UI) al pausar y reanudar el juego. Al pausar, se oculta la UI del jugador para evitar superposiciones con el menú de pausa, y al reanudar se vuelve a mostrar. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el menú de pausa funcione correctamente en relación con la visibilidad de la UI del jugador.</summary>
        [Export] private Lira _player;

        /// <summary>
        /// Se llama cuando el nodo entra en la escena. Configura el menú de pausa para que procese durante la pausa, asigna las funciones de los botones y oculta el menú inicialmente. También configura el fondo oscuro para que no bloquee los clicks sobre los botones del menú.
        /// </summary>
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
				_returnButton.Pressed += ResumeGame;

			if (_settingsButton != null)
				_settingsButton.Pressed += OpenSettings;

			if (_exitButton != null)
				_exitButton.Pressed += ExitToMenu;

			if (_optionsMenu != null)
			{
				_optionsMenu.Visible = false;
				_optionsMenu.ProcessMode = ProcessModeEnum.Always;
			}

			Visible = false;
		}

        /// <summary>
        /// Se llama para procesar eventos de entrada no manejados. Detecta si se ha presionado la tecla de pausa o escape para alternar el estado de pausa del juego. Si el juego ya está pausado y el menú de opciones está visible, lo oculta en lugar de reanudar el juego. Si el juego no está pausado, lo pausa y muestra el menú de pausa. Finalmente, marca el evento como manejado para evitar que otros nodos lo procesen.
        /// </summary>
        /// <param name="event">
        /// Evento de entrada recibido. Se verifica si corresponde a la acción de pausa o a la tecla escape para controlar el estado de pausa del juego y la visibilidad del menú de opciones.
        /// </param>
		public override void _UnhandledInput(InputEvent @event)
		{
			if (!IsPauseInput(@event))
				return;

			if (GetTree().Paused && _optionsMenu != null && _optionsMenu.Visible)
				_optionsMenu.Visible = false;
			else
				TogglePause();

			GetViewport().SetInputAsHandled();
		}

        /// <summary>
        /// Verifica si el evento de entrada corresponde a la acción de pausa o a la tecla escape. Permite detectar tanto la acción de pausa definida en el Input Map como la tecla escape para controlar el estado de pausa del juego. Esto proporciona flexibilidad para que el jugador pueda usar cualquiera de las dos opciones para pausar o reanudar el juego.
        /// </summary>
        /// <param name="event">
        /// Evento de entrada recibido. Se verifica si corresponde a la acción de pausa o a la tecla escape para determinar si se debe alternar el estado de pausa del juego.
        /// </param>
        /// <returns>
        /// Verdadero si el evento corresponde a la acción de pausa o a la tecla escape, indicando que se debe alternar el estado de pausa del juego; falso en caso contrario.
        /// </returns>
		private static bool IsPauseInput(InputEvent @event)
		{
			if (@event.IsActionPressed("pause") || @event.IsActionPressed("ui_cancel"))
				return true;

			if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
				return keyEvent.Keycode == Key.Escape;

			return false;
		}

        /// <summary>
        /// Aplica el modo de proceso "Always" de forma recursiva a un nodo y todos sus hijos. Esto asegura que el nodo y sus descendientes sigan procesando incluso cuando el juego está pausado, lo cual es necesario para que el menú de pausa y sus controles funcionen correctamente durante la pausa. Se llama desde el método _Ready para configurar el menú de pausa y sus controles para que procesen durante la pausa.
        /// </summary>
        /// <param name="node">
        /// Nodo al que se le aplicará el modo de proceso "Always" de forma recursiva. Se establece en el nodo raíz del menú de pausa para asegurar que todo el menú y sus controles sigan procesando durante la pausa.
        /// </param>
		private static void ApplyAlwaysProcessRecursive(Node node)
		{
			node.ProcessMode = ProcessModeEnum.Always;
			foreach (Node child in node.GetChildren())
			{
				ApplyAlwaysProcessRecursive(child);
			}
		}

        /// <summary>
        /// Alterna el estado de pausa del juego. Si el juego ya está pausado, lo reanuda; si no está pausado, lo pausa. Al pausar, muestra el menú de pausa y oculta la UI del jugador; al reanudar, oculta el menú de pausa y muestra la UI del jugador. Este método se llama desde el método _UnhandledInput cuando se detecta la entrada de pausa o escape para controlar el estado de pausa del juego.
        /// </summary>
		private void TogglePause()
		{
			if (GetTree().Paused)
			{
				ResumeGame();
				return;
			}

			PauseGame();
		}

        /// <summary>
        /// Pausa el juego y muestra el menú de pausa. Establece el estado de pausa del árbol de escenas, hace visible el menú de pausa, cambia el modo del mouse para mostrarlo y oculta la UI del jugador para evitar superposiciones con el menú. Este método se llama desde TogglePause cuando se detecta que el juego no está pausado y se desea pausar.
        /// </summary>
		private void PauseGame()
		{
			Visible = true;
			GetTree().Paused = true;
			Input.MouseMode = Input.MouseModeEnum.Visible;
            _player.VisibleUI(false);
		}

        /// <summary>
        /// Reanuda el juego desde el estado de pausa. Establece el estado de pausa del árbol de escenas a falso, oculta el menú de pausa, cambia el modo del mouse para ocultarlo y muestra la UI del jugador. Este método se llama desde TogglePause cuando se detecta que el juego ya está pausado y se desea reanudar, así como desde el botón de retorno en el menú de pausa para reanudar el juego al hacer clic en él.
        /// </summary>
		public void ResumeGame()
        {
            ButtonTools.PlayPressAnimation(
                _returnButton,
                () =>
                {
                    if (_optionsMenu != null)
                        _optionsMenu.Visible = false;

                    GetTree().Paused = false;
                    Visible = false;
                    _player.VisibleUI(true);
                });
        }

        /// <summary>
        /// Abre el menú de opciones dentro del menú de pausa. Al hacer clic en el botón de ajustes, se muestra el contenedor de opciones para que el jugador pueda interactuar con los ajustes del juego (audio, video, controles, etc.) incluso mientras el juego está pausado. Este método se llama desde el botón de ajustes en el menú de pausa para mostrar el menú de opciones al hacer clic en él.
        /// </summary>
		private void OpenSettings()
        {
            ButtonTools.PlayPressAnimation(
                _settingsButton,
                () =>
                {
                    _optionsMenu.Visible = true;
                });
        }

        /// <summary>
        /// Sale al menú principal desde el estado de pausa. Al hacer clic en el botón de salir, se detiene la pausa y se cambia la escena a la del menú principal. Este método se llama desde el botón de salir en el menú de pausa para salir al menú principal al hacer clic en él.
        /// </summary>
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

        /// <summary>
        /// Se llama cuando el nodo sale de la escena. Asegura que el juego no quede pausado si esta escena se libera mientras el juego está en pausa. Esto es importante para evitar que el juego quede en un estado de pausa indefinido si el menú de pausa se libera por alguna razón (por ejemplo, al cambiar de escena) mientras el juego está pausado. Al salir del árbol, verifica si el árbol de escenas aún existe y si está pausado; si es así, establece el estado de pausa a falso para reanudar el juego.
        /// </summary>
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

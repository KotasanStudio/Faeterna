using Faeterna.Scripts.Tools;
using Godot;

namespace Faeterna.Scripts.Menus
{
    /// <summary>
    /// Control que representa el menú principal del juego. Permite iniciar el juego, acceder a opciones y salir del juego. Al hacer clic en los botones, se reproducen animaciones de presión y se muestran los submenús correspondientes (opciones o selección de nivel). El fondo del menú se desenfoca al mostrar los submenús para mejorar la legibilidad. Se asignan las referencias a los botones y submenús desde el editor a través de export variables. Es importante que estas referencias estén correctamente asignadas para que el menú principal funcione correctamente. El menú principal es el punto de entrada al juego y proporciona acceso a las funciones principales antes de comenzar a jugar.
    /// </summary>
    public partial class MainMenu : Control
    {
        /// <summary>Botones del menú principal. Se asignan desde el editor a través de export variables. El botón de inicio inicia el juego y muestra el menú de selección de nivel, el botón de ajustes muestra el menú de opciones, y el botón de salir cierra el juego. Es importante que estas referencias estén correctamente asignadas para que los botones funcionen correctamente en el menú principal.</summary>
        [Export]  private TextureButton _startButton;

        /// <summary>Botón para acceder a las opciones del juego. Al hacer clic, muestra el menú de opciones dentro del menú principal. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el botón de ajustes funcione correctamente en el menú principal.</summary>
        [Export]  private TextureButton _settingsButton;

        /// <summary>Botón para salir del juego. Al hacer clic, cierra el juego. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el botón de salir funcione correctamente en el menú principal.</summary>
        [Export] private TextureButton _exitButton;

        /// <summary>Contenedor de controles para las opciones del juego (ajustes de audio, video, controles, etc.). Se muestra u oculta al interactuar con el botón de ajustes. Al mostrar el menú de opciones, se desenfoca el fondo para mejorar la legibilidad. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el menú de opciones funcione correctamente en el menú principal.</summary>
        [Export] private OptionsMenu _optionsMenu;

        /// <summary>Contenedor de controles para la selección de nivel o inicio del juego. Se muestra al hacer clic en el botón de inicio. Al mostrar este menú, se desenfoca el fondo para mejorar la legibilidad. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el menú de selección de nivel funcione correctamente en el menú principal.</summary>
        [Export] private PlayMenu _playMenu;

        /// <summary>Control que representa el fondo del menú principal. Se desenfoca al mostrar los submenús (opciones o selección de nivel) para mejorar la legibilidad. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el desenfoque del fondo funcione correctamente en el menú principal.</summary>
        [Export] private TextureRect _background;

        /// <summary>
        /// Se llama cuando el nodo entra en la escena. Configura el menú principal, asigna las funciones de los botones y oculta los submenús inicialmente. También configura el fondo para que no bloquee los clicks sobre los botones del menú.
        /// </summary>
        public override void _Ready()
        {
            GetNode<TextureRect>("TextureRect").MouseFilter = MouseFilterEnum.Ignore;
            _startButton.Pressed += OnPlayPressed;
            _settingsButton.Pressed += OnSettingsPressed;
            _exitButton.Pressed += OnExitPressed;
        }

        /// <summary>
        /// Se llama cada frame (non-physics). Controla la visibilidad de los botones principales y el desenfoque del fondo según la visibilidad de los submenús. Si no se muestra ningún submenú, se muestran los botones principales y se quita el desenfoque del fondo. Si se muestra algún submenú, se ocultan los botones principales y se aplica un desenfoque al fondo para mejorar la legibilidad de los submenús. Esta lógica asegura que el menú principal tenga una apariencia coherente y que los submenús sean fáciles de leer cuando están activos. Es importante que esta función esté correctamente implementada para que el menú principal funcione correctamente en términos de visibilidad y apariencia.
        /// </summary>
        /// <param name="delta">
        /// Tiempo en segundos desde el último frame. Se pasa a esta función para que pueda usarlo en su lógica de actualización, aunque en este caso no se utiliza directamente.
        /// </param>
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

        /// <summary>
        /// Función que se llama cuando se presiona el botón de inicio. Reproduce una animación de presión en el botón y luego muestra el menú de selección de nivel, ocultando los botones principales y aplicando un desenfoque al fondo para mejorar la legibilidad del submenú. Es importante que esta función esté correctamente implementada para que el botón de inicio funcione correctamente en el menú principal.
        /// </summary>
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
                    SetBlur(0.75f);
                    _playMenu.Visible = true;
                }
            );
        }

        /// <summary>
        /// Función que se llama cuando se presiona el botón de ajustes. Reproduce una animación de presión en el botón y luego muestra el menú de opciones, ocultando los botones principales y aplicando un desenfoque al fondo para mejorar la legibilidad del submenú. Es importante que esta función esté correctamente implementada para que el botón de ajustes funcione correctamente en el menú principal.
        /// </summary>
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
                    SetBlur(0.75f);
                    _optionsMenu.Visible = true;
                }
            );
        }

        /// <summary>
        /// Función que se llama cuando se presiona el botón de salir. Reproduce una animación de presión en el botón y luego cierra el juego. Es importante que esta función esté correctamente implementada para que el botón de salir funcione correctamente en el menú principal y permita cerrar el juego al hacer clic en él.
        /// </summary>
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

        /// <summary>
        /// Función que establece el nivel de desenfoque del fondo del menú principal. Al mostrar los submenús (opciones o selección de nivel), se aplica un desenfoque al fondo para mejorar la legibilidad de los submenús. Al ocultar los submenús, se quita el desenfoque para mostrar el fondo con claridad. Esta función se llama desde las funciones de los botones para aplicar o quitar el desenfoque según sea necesario. Es importante que esta función esté correctamente implementada para que el desenfoque del fondo funcione correctamente en el menú principal y mejore la apariencia de los submenús cuando están activos.
        /// </summary>
        /// <param name="intensidad">
        /// Nivel de desenfoque a aplicar al fondo. Un valor de 0 significa sin desenfoque, mientras que valores mayores aplican un desenfoque más fuerte. Este parámetro se utiliza para controlar la apariencia del fondo del menú principal cuando se muestran los submenús, mejorando la legibilidad de los mismos. Es importante que este parámetro se ajuste adecuadamente para lograr el efecto visual deseado en el menú principal.
        /// </param>
        private void SetBlur(float intensidad)
        {
            if (_background?.Material is ShaderMaterial shader)
            {
                shader.SetShaderParameter("intensidad", intensidad);
            }
        }
    }
}

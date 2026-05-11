using Faeterna.Scripts.Tools;
using Godot;

namespace Faeterna.Scripts.Menus
{
    /// <summary>
    /// Control que representa el menú de opciones del juego. Permite configurar ajustes de video (antialiasing, vsync, modo de pantalla, bloqueo de FPS, calidad de sombras) y audio (volumen maestro, música, efectos de sonido, ambiente, sonidos UI). Los cambios se aplican en tiempo real y se pueden guardar para futuras sesiones. El menú se integra con el sistema de guardado para cargar y guardar las opciones seleccionadas por el jugador. Se asignan los controles desde el editor a través de export variables para facilitar la conexión con la interfaz gráfica del menú.
    /// </summary>
    public partial class OptionsMenu : Control
    {
        [ExportGroup("Video")]

        /// <summary>Control para seleccionar el tipo de antialiasing. Permite elegir entre diferentes métodos de suavizado de bordes para mejorar la calidad visual del juego. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el control de antialiasing funcione correctamente en el menú de opciones.</summary>
        [Export] private OptionButton _antialiasing;

        /// <summary>
        /// Control para activar o desactivar VSync. Permite sincronizar la tasa de refresco del juego con la frecuencia de actualización del monitor para evitar el tearing. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el control de VSync funcione correctamente en el menú de opciones.
        /// </summary>
        [Export] private OptionButton _vsync;

        /// <summary>
        /// Control para seleccionar el modo de pantalla. Permite elegir entre modo ventana, pantalla completa o sin bordes. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el control de modo de pantalla funcione correctamente en el menú de opciones.
        /// </summary>
        [Export] private OptionButton _screen;

        /// <summary>Control para activar o desactivar el bloqueo de FPS. Permite limitar la tasa de cuadros por segundo del juego para mejorar el rendimiento o reducir el consumo de energía. Al activar el bloqueo de FPS, se habilitan controles adicionales para configurar el valor del límite de FPS. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el control de bloqueo de FPS funcione correctamente en el menú de opciones.</summary>
        [Export] private OptionButton _fpslock;

        /// <summary>Controles para configurar el valor del límite de FPS cuando el bloqueo de FPS está activado. Permiten ajustar el límite de cuadros por segundo para equilibrar el rendimiento y la calidad visual según las preferencias del jugador. Estos controles solo están activos cuando el bloqueo de FPS está habilitado. Se asignan desde el editor a través de export variables. Es importante que estas referencias estén correctamente asignadas para que los controles de configuración de FPS funcionen correctamente en el menú de opciones.</summary>
        [Export] private HSlider _fpsslider;

        /// <summary>Control para configurar el valor del límite de FPS cuando el bloqueo de FPS está activado. Permite ajustar el límite de cuadros por segundo para equilibrar el rendimiento y la calidad visual según las preferencias del jugador. Este control solo está activo cuando el bloqueo de FPS está habilitado. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el control de configuración de FPS funcione correctamente en el menú de opciones.</summary>
        [Export] private SpinBox _fpsspinbox;

        /// <summary>Control para seleccionar la calidad de las sombras. Permite elegir entre diferentes niveles de calidad de sombras para mejorar la apariencia visual del juego. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el control de calidad de sombras funcione correctamente en el menú de opciones.</summary>
        [Export] private OptionButton _shadows;

        [ExportGroup("Sound")]

        /// <summary>Control para ajustar el volumen maestro del juego. Permite configurar el nivel general de volumen que afecta a todos los sonidos del juego. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el control de volumen maestro funcione correctamente en el menú de opciones.</summary>
        [Export] private HSlider _master;

        /// <summary>Control para ajustar el volumen maestro del juego. Permite configurar el nivel general de volumen que afecta a todos los sonidos del juego. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el control de volumen maestro funcione correctamente en el menú de opciones.</summary>
        [Export] private SpinBox _masterSpinBox;

        /// <summary>Control para ajustar el volumen de la música del juego. Permite configurar el nivel de volumen específico para la música, lo que afecta a las pistas musicales del juego. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el control de volumen de música funcione correctamente en el menú de opciones.</summary>
        [Export] private HSlider _music;

        /// <summary>Control para ajustar el volumen de la música del juego. Permite configurar el nivel de volumen específico para la música, lo que afecta a las pistas musicales del juego. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el control de volumen de música funcione correctamente en el menú de opciones.</summary>
        [Export] private SpinBox _musicSpinBox;

        /// <summary>Control para ajustar el volumen de los efectos de sonido del juego. Permite configurar el nivel de volumen específico para los efectos de sonido, lo que afecta a los sonidos relacionados con acciones, interacciones y eventos en el juego. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el control de volumen de efectos de sonido funcione correctamente en el menú de opciones.</summary>
        [Export] private HSlider _soundfx;

        /// <summary>Control para ajustar el volumen de los efectos de sonido del juego. Permite configurar el nivel de volumen específico para los efectos de sonido, lo que afecta a los sonidos relacionados con acciones, interacciones y eventos en el juego. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el control de volumen de efectos de sonido funcione correctamente en el menú de opciones.</summary>
        [Export] private SpinBox _soundfxSpinBox;

        /// <summary>Control para ajustar el volumen de los sonidos ambientales del juego. Permite configurar el nivel de volumen específico para los sonidos relacionados con el ambiente, como el viento, la lluvia, los animales, etc. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el control de volumen de sonidos ambientales funcione correctamente en el menú de opciones.</summary>
        [Export] private HSlider _enviroment;

        /// <summary>Control para ajustar el volumen de los sonidos ambientales del juego. Permite configurar el nivel de volumen específico para los sonidos relacionados con el ambiente, como el viento, la lluvia, los animales, etc. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el control de volumen de sonidos ambientales funcione correctamente en el menú de opciones.</summary>
        [Export] private SpinBox _enviromentSpinBox;

        /// <summary>Control para ajustar el volumen de los sonidos de la interfaz de usuario (UI) del juego. Permite configurar el nivel de volumen específico para los sonidos relacionados con la interfaz, como clics, notificaciones, etc. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el control de volumen de sonidos UI funcione correctamente en el menú de opciones.</summary>
        [Export] private HSlider _uisound;

        /// <summary>Control para ajustar el volumen de los sonidos de la interfaz de usuario (UI) del juego. Permite configurar el nivel de volumen específico para los sonidos relacionados con la interfaz, como clics, notificaciones, etc. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el control de volumen de sonidos UI funcione correctamente en el menú de opciones.</summary>
        [Export] private SpinBox _uisoundSpinBox;

        [ExportGroup("Control buttons")]

        /// <summary> Botón para guardar los cambios realizados en las opciones. Al hacer clic, guarda la configuración actual en el sistema de guardado para que se mantenga en futuras sesiones. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el botón de guardar funcione correctamente en el menú de opciones.</summary>
        [Export] private TextureButton _saveButton;

        /// <summary>Botón para salir del menú de opciones. Al hacer clic, cierra el menú de opciones y vuelve al menú principal o a la escena anterior. Se asigna desde el editor a través de export variable. Es importante que esta referencia esté correctamente asignada para que el botón de salir funcione correctamente en el menú de opciones.</summary>
        [Export] private TextureButton _exitButton;

        /// <summary>Instancia de la clase de guardado para cargar y guardar las opciones del juego. Permite persistir la configuración seleccionada por el jugador entre sesiones. Se utiliza para cargar las opciones al iniciar el menú y para guardar las opciones cuando se presiona el botón de guardar. Es importante que esta instancia esté correctamente inicializada para que el sistema de guardado funcione correctamente en el menú de opciones.</summary>
        private Saves _saveSettings;

        /// <summary>Variable para rastrear si el bloqueo de FPS está activado. Se utiliza para habilitar o deshabilitar los controles de configuración de FPS y para aplicar el límite de FPS en tiempo real. Cuando el bloqueo de FPS está activado, se permite configurar el valor del límite de FPS; cuando está desactivado, se deshabilitan esos controles y se elimina cualquier límite de FPS aplicado. Es importante que esta variable esté correctamente gestionada para que la funcionalidad de bloqueo de FPS funcione correctamente en el menú de opciones.</summary>
        private bool _isFpslock = false;

        /// <summary>Variable para almacenar el valor actual del límite de FPS cuando el bloqueo de FPS está activado. Se utiliza para aplicar el límite de FPS en tiempo real y para mostrar el valor actual en los controles de configuración de FPS. Este valor se actualiza cuando el jugador ajusta el límite de FPS a través de los controles del menú, y se aplica al motor para limitar la tasa de cuadros por segundo del juego. Es importante que esta variable esté correctamente gestionada para que la funcionalidad de bloqueo de FPS funcione correctamente en el menú de opciones. El valor predeterminado se establece en 60 FPS, pero puede ser ajustado por el jugador según sus preferencias.</summary>
        private int _fpsValue = 60;

        /// <summary>
        /// Se llama cuando el nodo entra en la escena. Carga las opciones guardadas previamente utilizando la clase de guardado y aplica cada configuración a los controles correspondientes del menú. Esto asegura que el menú de opciones refleje la configuración actual del juego al abrirlo, permitiendo al jugador ver y ajustar sus preferencias de video y audio según lo que se haya guardado en sesiones anteriores. Es importante que esta función esté correctamente implementada para que el menú de opciones funcione correctamente en términos de carga y aplicación de configuraciones guardadas.
        /// </summary>
        public override void _Ready()
        {
            _saveSettings = new Saves(false);
            var options = _saveSettings.LoadOptions();

            // Video
            OnOptionButtonItemSelected(options.Antialiasing);
            OnVSyncButtonItemSelected(options.Vsync);
            OnWindowButtonItemSelected(options.Screen);
            _fpsValue = options.FpsValue;
            OnOptionFPSButtonItemSelected(options.FpsLock);
            OnOptionShadowButtonItemSelected(options.Shadows);

            // Sound
            OnHMasterSliderValueChanged(options.Master);
            OnHMusicSliderValueChanged(options.Music);
            OnHSoundFXSliderValueChanged(options.SoundFx);
            OnHEnviromentSliderValueChanged(options.Enviroment);
            OnHUISoundSliderValueChanged(options.UiSound);
        }

        /// <summary>
        /// Se llama cuando se selecciona una opción en el control de antialiasing. Aplica la configuración de antialiasing seleccionada al viewport del juego en tiempo real. Permite al jugador elegir entre diferentes métodos de suavizado de bordes para mejorar la calidad visual del juego según sus preferencias. Cada opción corresponde a un método específico de antialiasing, y al seleccionar una opción, se actualizan las configuraciones del viewport para reflejar esa elección. Es importante que esta función esté correctamente implementada para que el control de antialiasing funcione correctamente en el menú de opciones y para que los cambios se apliquen en tiempo real al juego.
        /// </summary>
        /// <param name="index">
        /// Índice de la opción seleccionada en el control de antialiasing. Cada índice corresponde a un método específico de antialiasing (por ejemplo, 0 para desactivado, 1 para FXAA, 2 para TAA, etc.). Este índice se utiliza para determinar qué configuración de antialiasing aplicar al viewport del juego. Es importante que los índices estén correctamente asignados en el control de antialiasing para que esta función funcione correctamente y aplique la configuración deseada según la selección del jugador.
        /// </param>
        public void OnOptionButtonItemSelected(int index)
        {
            _antialiasing.Selected = index;
            switch (index)
            {
                case 0:
                    GD.Print("Antialiasing: Off");
                    GetViewport().Msaa2D = Viewport.Msaa.Disabled;
                    GetViewport().Msaa3D = Viewport.Msaa.Disabled;
                    GetViewport().UseTaa = false;
                    GetViewport().ScreenSpaceAA = Viewport.ScreenSpaceAAEnum.Disabled;
                    break;
                case 1:
                    GD.Print("Antialiasing: FXAA");
                    GetViewport().Msaa2D = Viewport.Msaa.Disabled;
                    GetViewport().Msaa3D = Viewport.Msaa.Disabled;
                    GetViewport().UseTaa = false;
                    GetViewport().ScreenSpaceAA = Viewport.ScreenSpaceAAEnum.Fxaa;
                    break;
                case 2:
                    GD.Print("Antialiasing: TAA");
                    GetViewport().Msaa2D = Viewport.Msaa.Disabled;
                    GetViewport().Msaa3D = Viewport.Msaa.Disabled;
                    GetViewport().UseTaa = true;
                    GetViewport().ScreenSpaceAA = Viewport.ScreenSpaceAAEnum.Disabled;
                    break;
                case 3:
                    GD.Print("Antialiasing: MSAA 2x");
                    GetViewport().Msaa2D = Viewport.Msaa.Msaa2X;
                    GetViewport().Msaa3D = Viewport.Msaa.Msaa2X;
                    GetViewport().UseTaa = false;
                    GetViewport().ScreenSpaceAA = Viewport.ScreenSpaceAAEnum.Disabled;
                    break;
                case 4:
                    GD.Print("Antialiasing: MSAA 4x");
                    GetViewport().Msaa2D = Viewport.Msaa.Msaa4X;
                    GetViewport().Msaa3D = Viewport.Msaa.Msaa4X;
                    GetViewport().UseTaa = false;
                    GetViewport().ScreenSpaceAA = Viewport.ScreenSpaceAAEnum.Disabled;
                    break;
                case 5:
                    GD.Print("Antialiasing: MSAA 8x");
                    GetViewport().Msaa2D = Viewport.Msaa.Msaa8X;
                    GetViewport().Msaa3D = Viewport.Msaa.Msaa8X;
                    GetViewport().UseTaa = false;
                    GetViewport().ScreenSpaceAA = Viewport.ScreenSpaceAAEnum.Disabled;
                    break;
            }
        }

        /// <summary>
        /// Se llama cuando se selecciona una opción en el control de VSync. Aplica la configuración de VSync seleccionada al juego en tiempo real. Permite al jugador activar o desactivar la sincronización vertical para evitar el tearing y mejorar la calidad visual del juego según sus preferencias. Al seleccionar una opción, se actualiza la configuración de VSync del juego para reflejar esa elección. Es importante que esta función esté correctamente implementada para que el control de VSync funcione correctamente en el menú de opciones y para que los cambios se apliquen en tiempo real al juego.
        /// </summary>
        /// <param name="index">
        /// Índice de la opción seleccionada en el control de VSync. Por ejemplo, 0 para desactivado y 1 para activado. Este índice se utiliza para determinar si se debe habilitar o deshabilitar la sincronización vertical en el juego. Es importante que los índices estén correctamente asignados en el control de VSync para que esta función funcione correctamente y aplique la configuración deseada según la selección del jugador.
        /// </param>
        public void OnVSyncButtonItemSelected(int index)
        {
            _vsync.Selected = index;
            if (index == 0)
            {
                GD.Print("VSync: Off");
                DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
            }
            else if (index == 1)
            {
                GD.Print("VSync: On");
                DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
            }
        }

        /// <summary>
        /// Se llama cuando se selecciona una opción en el control de modo de pantalla. Aplica la configuración de modo de pantalla seleccionada al juego en tiempo real. Permite al jugador elegir entre diferentes modos de pantalla (ventana, pantalla completa, sin bordes) para adaptar la experiencia de juego a sus preferencias y necesidades. Al seleccionar una opción, se actualiza la configuración del modo de pantalla del juego para reflejar esa elección. Es importante que esta función esté correctamente implementada para que el control de modo de pantalla funcione correctamente en el menú de opciones y para que los cambios se apliquen en tiempo real al juego.
        /// </summary>
        /// <param name="index">
        /// Índice de la opción seleccionada en el control de modo de pantalla. Por ejemplo, 0 para modo ventana, 1 para pantalla completa y 2 para sin bordes. Este índice se utiliza para determinar qué modo de pantalla aplicar al juego. Es importante que los índices estén correctamente asignados en el control de modo de pantalla para que esta función funcione correctamente y aplique la configuración deseada según la selección del jugador.
        /// </param>
        public void OnWindowButtonItemSelected(int index)
        {
            _screen.Selected = index;
            switch (index)
            {
                case 0:
                    GD.Print("Screen Mode: Windowed");
                    DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                    break;
                case 1:
                    GD.Print("Screen Mode: Fullscreen");
                    DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                    break;
                case 2:
                    GD.Print("Screen Mode: Borderless");
                    DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
                    break;
            }
        }

        /// <summary>
        /// Se llama cuando se selecciona una opción en el control de bloqueo de FPS. Aplica la configuración de bloqueo de FPS seleccionada al juego en tiempo real. Permite al jugador activar o desactivar el bloqueo de FPS para limitar la tasa de cuadros por segundo del juego según sus preferencias. Al activar el bloqueo de FPS, se habilitan controles adicionales para configurar el valor del límite de FPS; al desactivarlo, se deshabilitan esos controles y se elimina cualquier límite de FPS aplicado. Es importante que esta función esté correctamente implementada para que el control de bloqueo de FPS funcione correctamente en el menú de opciones y para que los cambios se apliquen en tiempo real al juego.
        /// </summary>
        /// <param name="index">
        /// Índice de la opción seleccionada en el control de bloqueo de FPS. Por ejemplo, 0 para desactivado y 1 para activado. Este índice se utiliza para determinar si se debe habilitar o deshabilitar el bloqueo de FPS en el juego. Es importante que los índices estén correctamente asignados en el control de bloqueo de FPS para que esta función funcione correctamente y aplique la configuración deseada según la selección del jugador. Además, al activar el bloqueo de FPS, se deben habilitar los controles para configurar el valor del límite de FPS, y al desactivarlo, se deben deshabilitar esos controles y eliminar cualquier límite de FPS aplicado.
        /// </param>
        public void OnOptionFPSButtonItemSelected(int index)
        {
            _fpslock.Selected = index;
            if (index == 0)
            {
                GD.Print("FPS Lock: Off");
                _isFpslock = false;
                Engine.MaxFps = 0;
                _fpsslider.Editable = false;
                _fpsspinbox.Editable = false;
            }
            else if (index == 1)
            {
                GD.Print("FPS Lock: On");
                _isFpslock = true;
                Engine.MaxFps = _fpsValue;
                _fpsslider.Value = _fpsValue;
                _fpsspinbox.Value = _fpsValue;
                _fpsslider.Editable = true;
                _fpsspinbox.Editable = true;
            }
        }

        /// <summary>
        /// Se llama cuando se cambia el valor del control de configuración de FPS (slider o spinbox). Aplica el nuevo valor del límite de FPS al juego en tiempo real. Permite al jugador ajustar el límite de cuadros por segundo para equilibrar el rendimiento y la calidad visual según sus preferencias. Este método se llama tanto desde el slider como desde el spinbox para asegurarse de que ambos controles estén sincronizados y reflejen el mismo valor. Es importante que esta función esté correctamente implementada para que los controles de configuración de FPS funcionen correctamente en el menú de opciones y para que los cambios se apliquen en tiempo real al juego cuando el bloqueo de FPS está activado.
        /// </summary>
        /// <param name="value">
        /// Nuevo valor del límite de FPS seleccionado por el jugador a través del slider o spinbox. Este valor se utiliza para actualizar la configuración de FPS del juego en tiempo real, permitiendo al jugador ajustar la tasa de cuadros por segundo según sus preferencias. Es importante que este valor se gestione correctamente para que los controles de configuración de FPS funcionen correctamente en el menú de opciones y para que los cambios se apliquen en tiempo real al juego cuando el bloqueo de FPS está activado. Además, este valor debe mantenerse sincronizado entre el slider y el spinbox para reflejar la misma configuración de FPS en ambos controles.
        /// </param>
        public void OnHSliderFPSValueChanged(float value)
        {
            if (_isFpslock)
            {
                _fpsValue = (int)value;
                _fpsspinbox.Value = _fpsValue;
                _fpsslider.Value = _fpsValue;
                Engine.MaxFps = _fpsValue;
            }
        }

        /// <summary>
        /// Se llama cuando se cambia el valor del control de configuración de FPS (slider o spinbox). Aplica el nuevo valor del límite de FPS al juego en tiempo real. Permite al jugador ajustar el límite de cuadros por segundo para equilibrar el rendimiento y la calidad visual según sus preferencias. Este método se llama tanto desde el slider como desde el spinbox para asegurarse de que ambos controles estén sincronizados y reflejen el mismo valor. Es importante que esta función esté correctamente implementada para que los controles de configuración de FPS funcionen correctamente en el menú de opciones y para que los cambios se apliquen en tiempo real al juego cuando el bloqueo de FPS está activado.
        /// </summary>
        /// <param name="value">
        /// Nuevo valor del límite de FPS seleccionado por el jugador a través del slider o spinbox. Este valor se utiliza para actualizar la configuración de FPS del juego en tiempo real, permitiendo al jugador ajustar la tasa de cuadros por segundo según sus preferencias. Es importante que este valor se gestione correctamente para que los controles de configuración de FPS funcionen correctamente en el menú de opciones y para que los cambios se apliquen en tiempo real al juego cuando el bloqueo de FPS está activado. Además, este valor debe mantenerse sincronizado entre el slider y el spinbox para reflejar la misma configuración de FPS en ambos controles.
        /// </param>
        public void OnSpinFPSBoxValueChanged(float value)
        {
            if (_isFpslock)
            {
                _fpsValue = (int)value;
                _fpsspinbox.Value = _fpsValue;
                _fpsslider.Value = _fpsValue;
                Engine.MaxFps = _fpsValue;
            }
        }

        /// <summary>
        /// Se llama cuando se selecciona una opción en el control de calidad de sombras. Aplica la configuración de calidad de sombras seleccionada al juego en tiempo real. Permite al jugador elegir entre diferentes niveles de calidad de sombras para mejorar la apariencia visual del juego según sus preferencias. Al seleccionar una opción, se actualiza la configuración de calidad de sombras del juego para reflejar esa elección. Es importante que esta función esté correctamente implementada para que el control de calidad de sombras funcione correctamente en el menú de opciones y para que los cambios se apliquen en tiempo real al juego. Además, esta función maneja posibles excepciones al aplicar la configuración de sombras, especialmente en entornos sin backend gráfico, para garantizar que el juego no se bloquee y que se informe adecuadamente al jugador si no se pueden aplicar las configuraciones seleccionadas.
        /// </summary>
        /// <param name="index">
        /// Índice de la opción seleccionada en el control de calidad de sombras. Cada índice corresponde a un nivel específico de calidad de sombras (por ejemplo, 0 para Hard, 1 para Soft Very Low, 2 para Soft Low, etc.). Este índice se utiliza para determinar qué configuración de calidad de sombras aplicar al juego. Es importante que los índices estén correctamente asignados en el control de calidad de sombras para que esta función funcione correctamente y aplique la configuración deseada según la selección del jugador. Además, esta función debe manejar posibles excepciones al aplicar la configuración de sombras, especialmente en entornos sin backend gráfico, para garantizar que el juego no se bloquee y que se informe adecuadamente al jugador si no se pueden aplicar las configuraciones seleccionadas.
        /// </param>
        public void OnOptionShadowButtonItemSelected(int index)
        {
            _shadows.Selected = index;
            switch (index)
            {
                case 0:
                    GD.Print("Shadow Quality: Hard");
                    ApplyShadowQuality(RenderingServer.ShadowQuality.Hard);
                    break;
                case 1:
                    GD.Print("Shadow Quality: Soft Very Low");
                    ApplyShadowQuality(RenderingServer.ShadowQuality.SoftVeryLow);
                    break;
                case 2:
                    GD.Print("Shadow Quality: Soft Low");
                    ApplyShadowQuality(RenderingServer.ShadowQuality.SoftLow);
                    break;
                case 3:
                    GD.Print("Shadow Quality: Soft Medium");
                    ApplyShadowQuality(RenderingServer.ShadowQuality.SoftMedium);
                    break;
                case 4:
                    GD.Print("Shadow Quality: Soft High");
                    ApplyShadowQuality(RenderingServer.ShadowQuality.SoftHigh);
                    break;
                case 5:
                    GD.Print("Shadow Quality: Soft Ultra");
                    ApplyShadowQuality(RenderingServer.ShadowQuality.SoftUltra);
                    break;
            }
        }

        /// <summary>
        /// Aplica la configuración de calidad de sombras seleccionada al juego en tiempo real. Permite al jugador elegir entre diferentes niveles de calidad de sombras para mejorar la apariencia visual del juego según sus preferencias. Al seleccionar una opción, se actualiza la configuración de calidad de sombras del juego para reflejar esa elección. Es importante que esta función esté correctamente implementada para que el control de calidad de sombras funcione correctamente en el menú de opciones y para que los cambios se apliquen en tiempo real al juego. Además, esta función maneja posibles excepciones al aplicar la configuración de sombras, especialmente en entornos sin backend gráfico, para garantizar que el juego no se bloquee y que se informe adecuadamente al jugador si no se pueden aplicar las configuraciones seleccionadas.
        /// </summary>
        /// <param name="quality">
        /// Configuración de calidad de sombras seleccionada por el jugador. Este parámetro se utiliza para determinar qué nivel de calidad de sombras aplicar al juego. Cada nivel de calidad de sombras corresponde a una configuración específica que afecta la apariencia visual de las sombras en el juego. Es importante que esta función maneje posibles excepciones al aplicar la configuración de sombras, especialmente en entornos sin backend gráfico, para garantizar que el juego no se bloquee y que se informe adecuadamente al jugador si no se pueden aplicar las configuraciones seleccionadas.
        /// </param>
        private static void ApplyShadowQuality(RenderingServer.ShadowQuality quality)
        {
            // En entornos sin backend gráfico, RenderingServer puede no estar disponible.
            if (DisplayServer.GetName() == "headless")
            {
                return;
            }

            try
            {
                RenderingServer.DirectionalSoftShadowFilterSetQuality(quality);
                RenderingServer.PositionalSoftShadowFilterSetQuality(quality);
            }
            catch (System.Exception ex)
            {
                GD.PushWarning($"No se pudo aplicar la calidad de sombras: {ex.Message}");
            }
        }

        private float NormalizeVolumeToDb(float percentValue)
        {
            float normalizedValue = Mathf.Clamp(percentValue / 100f, 0f, 1f);
            if (normalizedValue <= 0)
            {
                return -80f;
            }
            return Mathf.LinearToDb(normalizedValue);
        }

        public void OnHMasterSliderValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(0, NormalizeVolumeToDb(value));
            _masterSpinBox.Value = value;
            _master.Value = value;
        }

        public void OnHMusicSliderValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(1, NormalizeVolumeToDb(value));
            _musicSpinBox.Value = value;
            _music.Value = value;
        }

        public void OnHSoundFXSliderValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(2, NormalizeVolumeToDb(value));
            _soundfxSpinBox.Value = value;
            _soundfx.Value = value;
        }

        public void OnHEnviromentSliderValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(3, NormalizeVolumeToDb(value));
            _enviromentSpinBox.Value = value;
            _enviroment.Value = value;
        }

        public void OnHUISoundSliderValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(4, NormalizeVolumeToDb(value));
            _uisoundSpinBox.Value = value;
            _uisound.Value = value;
        }

        public void OnSpinBoxMasterValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(0, NormalizeVolumeToDb(value));
            _masterSpinBox.Value = value;
            _master.Value = value;
        }

        public void OnSpinBoxMusicValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(1, NormalizeVolumeToDb(value));
            _music.Value = value;
            _musicSpinBox.Value = value;
        }

        public void OnSpinBoxSoundFXValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(2, NormalizeVolumeToDb(value));
            _soundfx.Value = value;
            _soundfxSpinBox.Value = value;
        }

        public void OnSpinBoxEnviromentValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(3, NormalizeVolumeToDb(value));
            _enviroment.Value = value;
            _enviromentSpinBox.Value = value;
        }

        public void OnSpinBoxUISoundValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(4, NormalizeVolumeToDb(value));
            _uisound.Value = value;
            _uisoundSpinBox.Value = value;
        }

        public void OnSavePressed()
        {
            ButtonTools.PlayPressAnimation(
                _saveButton,
                () =>
                {
                    _saveSettings.SaveOptions(
                        _antialiasing.Selected,
                        _vsync.Selected,
                        _screen.Selected,
                        _fpslock.Selected,
                        _fpsValue,
                        _shadows.Selected,
                        _master.Value,
                        _music.Value,
                        _soundfx.Value,
                        _enviroment.Value,
                        _uisound.Value
                    );
                }
            );
        }

        public void OnExitPressed()
        {
            ButtonTools.PlayPressAnimation(
                _exitButton,
                () =>
                {
                    Visible = false;
                }
            );
        }
    }
}

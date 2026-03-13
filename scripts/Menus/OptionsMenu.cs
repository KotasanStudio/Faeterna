using Faeterna.scripts.Tools;
using Godot;

namespace Faeterna.scripts.Menus
{
    public partial class OptionsMenu : Control
    {
        [ExportGroup("Video")]
        [Export]
        private OptionButton _antialiasing;

        [Export]
        private OptionButton _vsync;

        [Export]
        private OptionButton _screen;

        [Export]
        private OptionButton _fpslock;

        [Export]
        private HSlider _fpsslider;

        [Export]
        private SpinBox _fpsspinbox;

        [Export]
        private OptionButton _shadows;

        [ExportGroup("Sound")]
        [Export]
        private HSlider _master;

        [Export]
        private SpinBox _masterSpinBox;

        [Export]
        private HSlider _music;

        [Export]
        private SpinBox _musicSpinBox;

        [Export]
        private HSlider _soundfx;

        [Export]
        private SpinBox _soundfxSpinBox;

        [Export]
        private HSlider _enviroment;

        [Export]
        private SpinBox _enviromentSpinBox;

        [Export]
        private HSlider _uisound;

        [Export]
        private SpinBox _uisoundSpinBox;

        [ExportGroup("Control buttons")]
        [Export]
        private TextureButton _saveButton;

        [Export]
        private TextureButton _exitButton;

        private Saves _saveSettings;
        private bool _isFpslock = false;
        private int _fpsValue = 60;

        // Called when the node enters the scene tree for the first time.
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

        public void OnHMasterSliderValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb(value));
            _masterSpinBox.Value = value;
            _master.Value = value;
        }

        public void OnHMusicSliderValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(1, Mathf.LinearToDb(value));
            _musicSpinBox.Value = value;
            _music.Value = value;
        }

        public void OnHSoundFXSliderValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(2, Mathf.LinearToDb(value));
            _soundfxSpinBox.Value = value;
            _soundfx.Value = value;
        }

        public void OnHEnviromentSliderValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(3, Mathf.LinearToDb(value));
            _enviromentSpinBox.Value = value;
            _enviroment.Value = value;
        }

        public void OnHUISoundSliderValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(4, Mathf.LinearToDb(value));
            _uisoundSpinBox.Value = value;
            _uisound.Value = value;
        }

        public void OnSpinBoxMasterValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb(value));
            _masterSpinBox.Value = value;
            _master.Value = value;
        }

        public void OnSpinBoxMusicValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(1, Mathf.LinearToDb(value));
            _music.Value = value;
            _musicSpinBox.Value = value;
        }

        public void OnSpinBoxSoundFXValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(2, Mathf.LinearToDb(value));
            _soundfx.Value = value;
            _soundfxSpinBox.Value = value;
        }

        public void OnSpinBoxEnviromentValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(3, Mathf.LinearToDb(value));
            _enviroment.Value = value;
            _enviromentSpinBox.Value = value;
        }

        public void OnSpinBoxUISoundValueChanged(float value)
        {
            AudioServer.SetBusVolumeDb(4, Mathf.LinearToDb(value));
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

using Godot;
using System;

namespace Faeterna.scripts.Tools
{
    public record OptionsData(
        int Antialiasing,
        int Vsync,
        int Screen,
        int FpsLock,
        int FpsValue,
        int Shadows,
        float Master,
        float Music,
        float SoundFx,
        float Enviroment,
        float UiSound);

    public class Saves
    {
        ConfigFile config = new ConfigFile();

        public Saves(bool isSave)
        {
            if (isSave)
            {
                try
                {

                }
                catch (Exception)
                {

                }
            }
            else
            {
                Load("user://settings.cfg");
            }
        }

        public void Load(string filename)
        {
            try
            {
                config.Load(filename);
            }
            catch (Exception)
            {
                config.Save(filename);
            }
        }

        public void SaveOptions(
            int antialiasing,
            int vsync,
            int screen,
            int fpslock,
            int fpsValue,
            int shadows,
            double master,
            double music,
            double soundfx,
            double enviroment,
            double uisound)
        {
            config.SetValue("Video", "antialiasing", antialiasing);
            config.SetValue("Video", "vsync", vsync);
            config.SetValue("Video", "screen", screen);
            config.SetValue("Video", "fpslock", fpslock);
            config.SetValue("Video", "fps_value", fpsValue);
            config.SetValue("Video", "shadows", shadows);

            config.SetValue("Sound", "master", master);
            config.SetValue("Sound", "music", music);
            config.SetValue("Sound", "soundfx", soundfx);
            config.SetValue("Sound", "enviroment", enviroment);
            config.SetValue("Sound", "uisound", uisound);

            config.Save("user://settings.cfg");
        }

        public OptionsData LoadOptions()
        {
            return new OptionsData(
                (int)config.GetValue("Video", "antialiasing", 0),
                (int)config.GetValue("Video", "vsync", 0),
                (int)config.GetValue("Video", "screen", 0),
                (int)config.GetValue("Video", "fpslock", 0),
                (int)config.GetValue("Video", "fps_value", 60),
                (int)config.GetValue("Video", "shadows", 0),
                (float)(double)config.GetValue("Sound", "master", 1.0),
                (float)(double)config.GetValue("Sound", "music", 1.0),
                (float)(double)config.GetValue("Sound", "soundfx", 1.0),
                (float)(double)config.GetValue("Sound", "enviroment", 1.0),
                (float)(double)config.GetValue("Sound", "uisound", 1.0)
            );
        }
    }
}

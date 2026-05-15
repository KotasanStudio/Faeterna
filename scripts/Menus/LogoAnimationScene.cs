using Godot;
using System;

public partial class LogoAnimationScene : Control
{
	[Export] private Timer logoTimer;
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("pause"))
        {
			logoTimer.Stop();
             GetTree().ChangeSceneToFile("res://scenes/Menus/MainMenu.tscn");
        }
    }
	public void _on_logo_timer_timeout()
    {
        GetTree().ChangeSceneToFile("res://scenes/Menus/MainMenu.tscn");
    }
}

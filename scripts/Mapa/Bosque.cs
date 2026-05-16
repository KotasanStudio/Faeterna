using Godot;
using System;

/// <summary>
/// Escena del bosque del juego. Gestiona la reproducción de la música de fondo
/// cuando el jugador entra en esta escena. Se asigna la referencia al reproductor
/// de audio desde el editor a través de una export variable.
/// </summary>
public partial class Bosque : Node2D
{
    /// <summary>Reproductor de audio para la música de fondo del bosque. Se asigna desde el editor.</summary>
    [Export] private AudioStreamPlayer _backgroundMusic;

    /// <summary>
    /// Inicialización del nodo. Se llama cuando la escena entra en el árbol.
    /// Inicia la reproducción de la música de fondo si está correctamente asignada.
    /// Si el reproductor de audio no está asignado, registra un error.
    /// </summary>
    public override void _Ready()
    {
        if (_backgroundMusic != null)
        {
            _backgroundMusic.Play();
        }
        else
        {
            GD.PrintErr("AudioStreamPlayer2D para música de fondo no asignado en Bosque");
        }
    }

    /// <summary>
    /// Se llama cada frame (física no incluida). Actualmente no implementa lógica adicional.
    /// Puede ser utilizado en el futuro para lógica de actualización del bosque.
    /// </summary>
    /// <param name="delta">Tiempo en segundos desde el último frame.</param>
    public override void _Process(double delta)
    {
    }
}

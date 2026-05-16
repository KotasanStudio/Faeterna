using System;
using Godot;

namespace Faeterna.Scripts.Tools
{
    /// <summary>
    /// Utilidad estática que proporciona funcionalidades comunes para animaciones y efectos visuales en botones.
    /// Ofrece métodos para crear animaciones de presión fluidas que mejoran la retroalimentación visual del usuario
    /// cuando interactúa con botones del menú y otros controles de interfaz.
    /// </summary>
    public static class ButtonTools
    {
        /// <summary>Escala a la que se reduce el botón cuando se presiona (enciogimiento).</summary>
        private static readonly Vector2 PressedScale = new(0.85f, 0.85f);

        /// <summary>Escala a la que rebota el botón después de presionarse (mayor que normal).</summary>
        private static readonly Vector2 BounceScale = new(1.1f, 1.1f);

        /// <summary>Escala normal del botón (sin transformar).</summary>
        private static readonly Vector2 NormalScale = Vector2.One;

        /// <summary>Duración en segundos de la fase de presión (enciogimiento) del botón.</summary>
        private const float PressDuration = 0.1f;

        /// <summary>Duración en segundos de la fase de rebote del botón.</summary>
        private const float BounceDuration = 0.12f;

        /// <summary>Duración en segundos de la fase de asentamiento en la escala normal.</summary>
        private const float SettleDuration = 0.08f;

        /// <summary>
        /// Reproduce una animación de "press" en el botón: se encoge, luego rebota más grande de lo normal y vuelve a su tamaño original.
        /// Al terminar la animación ejecuta el callback <paramref name="onFinished"/> si se proporciona.
        /// No requiere que el método que lo llame sea async.
        /// </summary>
        /// <param name="button">Control (botón) al que se le aplicará la animación de presión.</param>
        /// <param name="onFinished">Callback opcional que se ejecutará cuando la animación termine completamente.</param>
        public static void PlayPressAnimation(Control button, Action onFinished = null)
        {
            // Asegurarse de que el pivot esté en el centro del botón
            button.PivotOffset = button.Size / 2;

            var tween = button.CreateTween();

            // Paso 1: Encoger
            tween.TweenProperty(button, "scale", PressedScale, PressDuration)
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Cubic);

            // Paso 2: Rebotar haciéndose más grande
            tween.TweenProperty(button, "scale", BounceScale, BounceDuration)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);

            // Paso 3: Volver al tamaño normal
            tween.TweenProperty(button, "scale", NormalScale, SettleDuration)
                .SetEase(Tween.EaseType.InOut)
                .SetTrans(Tween.TransitionType.Cubic);

            // Ejecutar el callback cuando termine la animación
            if (onFinished != null)
            {
                tween.TweenCallback(Callable.From(onFinished));
            }
        }
    }
}

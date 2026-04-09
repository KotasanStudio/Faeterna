using System;
using Godot;

namespace Faeterna.scripts.Tools
{
    public static class ButtonTools
    {
        private static readonly Vector2 PressedScale = new(0.85f, 0.85f);
        private static readonly Vector2 BounceScale = new(1.1f, 1.1f);
        private static readonly Vector2 NormalScale = Vector2.One;

        private const float PressDuration = 0.1f;
        private const float BounceDuration = 0.12f;
        private const float SettleDuration = 0.08f;

        /// <summary>
        /// Reproduce una animación de "press" en el botón: se encoge, luego rebota
        /// más grande de lo normal y vuelve a su tamaño original.
        /// Al terminar la animación ejecuta el callback <paramref name="onFinished"/> si se proporciona.
        /// No requiere que el método que lo llame sea async.
        /// </summary>
        public static void PlayPressAnimation(Control button, Action onFinished = null)
        {
            // Asegurarse de que el pivot esté en el centro del botón
            button.PivotOffset = button.Size / 2;

            var tween = button.CreateTween();

            // Paso 1: Encoger (irse para dentro)
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

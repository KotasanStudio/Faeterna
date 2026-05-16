using Godot;
using System;

namespace Faeterna.Scripts.Personaje
{
    public partial class Shot : Area2D
    {
        /// <summary>
        /// Velocidad a la que se mueve la bala. Se puede ajustar en el editor para equilibrar el juego. Un valor típico podría ser 450, pero se recomienda probar diferentes velocidades para encontrar la que mejor se adapte al diseño del juego y a la sensación de disparo deseada.
        /// </summary>
        [Export] private float Speed = 450f;

        /// <summary>
        /// Referencia al nodo AnimatedSprite2D que se usará para mostrar la animación de la bala. Se asigna en el editor y se utiliza para cambiar la orientación de la animación según la dirección del disparo. Es importante asegurarse de que el nodo AnimatedSprite2D esté correctamente configurado con las animaciones necesarias para que la bala se vea bien en movimiento.
        /// </summary>
        [Export] private AnimatedSprite2D _animatedSprite;

        /// <summary>
        /// Dirección en la que se mueve la bala. Se establece al crear la instancia de Shot y determina hacia dónde se desplazará. Es importante normalizar este vector para asegurar que la velocidad de la bala sea consistente sin importar la dirección. Por ejemplo, si el jugador dispara hacia arriba, la dirección podría ser (0, -1), mientras que si dispara hacia abajo, sería (0, 1). Para disparar hacia la derecha, sería (1, 0) y hacia la izquierda (-1, 0). Si se desea permitir disparos en ángulos diagonales, se pueden usar valores como (0.707f, -0.707f) para un disparo hacia arriba a la derecha.
        /// </summary>
        public Vector2 Direction = Vector2.Zero;

        /// <summary>Costo de mana para disparar esta bala. Este valor se puede ajustar en el editor para equilibrar el juego. Un valor típico podría ser 20, pero se recomienda probar diferentes costos para encontrar el equilibrio adecuado entre el uso de habilidades y la gestión de recursos del jugador. Es importante que este costo sea lo suficientemente alto como para hacer que el jugador piense estratégicamente sobre cuándo usar su habilidad de disparo, pero no tan alto como para desincentivar su uso por completo.</summary>
        public float ManaCost = 20f;

        /// <summary>
        /// En el método _Ready se pueden realizar configuraciones iniciales para la bala, como asegurarse de que el nodo AnimatedSprite2D esté asignado correctamente y configurar cualquier animación o efecto visual necesario. También se pueden conectar señales para detectar colisiones con otros objetos, como el terreno o enemigos, lo que permitirá manejar la lógica de impacto y destrucción de la bala de manera adecuada. Es importante que este método esté bien implementado para garantizar que la bala funcione correctamente desde el momento en que es creada en el juego.
        /// </summary>
        public override void _Ready()
        {
        }

        /// <summary>
        /// En el método _Process se maneja el movimiento de la bala. Se verifica si la dirección es diferente de cero para evitar movimientos innecesarios. Luego, se traduce la posición de la bala en función de su dirección, velocidad y el tiempo delta para asegurar un movimiento suave y consistente. Además, se ajusta la orientación del sprite de la bala según la dirección del disparo, volteándolo horizontalmente si se dispara hacia la izquierda. Es importante que este método esté bien implementado para garantizar que la bala se mueva correctamente y tenga una apariencia adecuada durante su trayectoria.
        /// </summary>
        /// <param name="delta">
        /// Tiempo en segundos desde el último frame. Este valor se utiliza para calcular el movimiento de la bala de manera independiente de la tasa de frames, asegurando que la velocidad de la bala sea consistente sin importar el rendimiento del juego. Es importante usar este valor para multiplicar la velocidad de la bala, ya que esto permitirá que el movimiento sea suave y fluido incluso en situaciones donde la tasa de frames pueda variar.
        /// </param>
        public override void _Process(double delta)
        {
            if (Direction == Vector2.Zero)
                return;

            Translate(Direction * Speed * (float)delta);
            if (Direction.X < 0f)
                _animatedSprite.FlipH = true;
            else if (Direction.X > 0f)
                _animatedSprite.FlipH = false;

        }

        /// <summary>
        /// Handles logic when another Area2D enters the monitored area.
        /// </summary>
        /// <param name="area">The Area2D instance that has entered the area. Cannot be null.</param>
        private void OnAreaEntered(Area2D area)
        {
            HandleCollision(area);
        }

        /// <summary>Elimina la bala al chocar con el terreno.</summary>
        private void OnBodyEntered(Node body)
        {
            HandleCollision(body);
        }

        /// <summary>
        /// Handles collision logic for both Area2D and PhysicsBody2D nodes. If the collided node is part of the "Terreno" group or is named "Hurtbox", the bullet will be removed from the scene. This method centralizes collision handling to avoid code duplication between area and body collisions.
        /// </summary>
        /// <param name="node">
        /// The Node instance that has collided with the bullet. This can be either an Area2D or a PhysicsBody2D, depending on the type of collision detected. The method checks the groups and name of the node to determine if it should trigger the destruction of the bullet.
        /// </param>
        private void HandleCollision(Node node)
        {
            GD.Print($"Shot collided with: {node.GetGroups()}");
            if (node.IsInGroup("Terreno") || node.Name == "Hurtbox")
            {
                QueueFree();
                return;
            }
        }
    }
}

using Faeterna.scripts.Personaje;
using Godot;

namespace Faeterna.Scripts.Personaje
{
    public partial class LiraInventory : Node
    {
        /// <summary>
        /// Nodo que representa el inventario de Lira. Este nodo se encarga de gestionar los objetos y recursos que Lira puede recoger durante el juego. Es importante que este nodo esté correctamente configurado para permitir a Lira interactuar con los objetos del mundo y almacenar los recursos que recoja. Asegúrate de implementar las funciones necesarias para agregar, eliminar y consultar los objetos en el inventario, así como para actualizar la interfaz de usuario si es necesario.
        /// </summary>
        public static bool dobleJump = false;
        public static bool dash = false;

        public void setDobleJump(bool value)
        {
            dobleJump = value;
        }
        public void setDash(bool value)
        {
            dash = value;
        }
        public bool getDobleJump()
        {
            return dobleJump;
        }
        public bool getDash()
        {
            return dash;
        }
    }
}
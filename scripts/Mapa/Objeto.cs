using Faeterna.Scripts.Personaje;
using Godot;
using System;
using System.Collections.Generic;

namespace Faeterna.scripts.Mapa
{
    /// <summary>
    /// Objeto coleccionable del mapa. Carga sus datos desde el CSV de items,
    /// muestra un efecto visual de luz y, al interactuar con él, otorga una habilidad
    /// o abre la descripción del objeto recogido.
    /// </summary>
    public partial class Objeto : Node2D
    {
        /// <summary>
        /// Ruta al archivo CSV que contiene los datos de los items.
        /// </summary>
        private const string ItemsCsvPath = "res://data/items.csv";

        /// <summary>
        /// Identificador del item que representa este objeto en el CSV.
        /// </summary>
        [Export] private int _itemId;

        /// <summary>
        /// Indica si este objeto forma parte del tutorial.
        /// </summary>
        [Export] private bool _isTutorialItem;

        /// <summary>
        /// Nodo auxiliar que se muestra u oculta cuando el objeto pertenece al tutorial.
        /// </summary>
        [Export] private Node2D _tutorial;

        /// <summary>
        /// Datos visibles del item cargado desde el CSV.
        /// </summary>
        [ExportGroup("Datos del ítem")]
        [Export] private string _itemName;

        /// <summary>
        /// Ruta del recurso visual o animación del item.
        /// </summary>
        [Export] private string _animationPath;

        /// <summary>
        /// Descripción textual del item.
        /// </summary>
        [Export] private string _itemDescription;

        /// <summary>
        /// Historia o lore asociado al item.
        /// </summary>
        [Export] private string _itemHistory;

        /// <summary>
        /// Efecto visual de iluminación que rodea al item.
        /// </summary>
        [ExportGroup("Animación de Aro de Luz")]
        [Export] private PointLight2D _aroLuz;

        /// <summary>
        /// Escala mínima del efecto de luz.
        /// </summary>
        [Export] private float _escalaMinima = 0.1f;

        /// <summary>
        /// Escala máxima del efecto de luz.
        /// </summary>
        [Export] private float _escalaMaxima = 1.0f;

        /// <summary>
        /// Duración completa del ciclo de animación de la luz.
        /// </summary>
        [Export] private float _duracionAnimacion = 2.0f;

        /// <summary>
        /// Tiempo acumulado para la animación del efecto de luz.
        /// </summary>
        private float _tiempoTranscurrido;

        /// <summary>
        /// Caché estática con los datos de todos los items cargados desde el CSV.
        /// </summary>
        private static readonly Dictionary<int, ItemData> ItemsCache = new();

        /// <summary>
        /// Indica si el CSV ya fue cargado previamente.
        /// </summary>
        private static bool _itemsCargados;

        /// <summary>
        /// Nombre visible del item cargado.
        /// </summary>
        public string ItemName => _itemName;

        /// <summary>
        /// Ruta del recurso visual del item cargado.
        /// </summary>
        public string AnimationPath => _animationPath;

        /// <summary>
        /// Descripción visible del item cargado.
        /// </summary>
        public string ItemDescription => _itemDescription;

        /// <summary>
        /// Historia visible del item cargado.
        /// </summary>
        public string ItemHistory => _itemHistory;

        /// <summary>
        /// Inicializa el objeto, carga los datos del item y configura el efecto visual.
        /// </summary>
        public override void _Ready()
        {
            CargarDatosDelItem();

            if (_aroLuz != null)
            {
                _aroLuz.TextureScale = _escalaMinima;
            }

            _tutorial.Visible = _isTutorialItem;
        }

        /// <summary>
        /// Actualiza la animación del aro de luz del objeto.
        /// </summary>
        /// <param name="delta">Tiempo transcurrido desde el último frame.</param>
        public override void _Process(double delta)
        {
            if (_aroLuz == null)
            {
                return;
            }

            _tiempoTranscurrido += (float)delta;

            float progreso = (_tiempoTranscurrido % _duracionAnimacion) / _duracionAnimacion;
            _aroLuz.TextureScale = Mathf.Lerp(_escalaMinima, _escalaMaxima, progreso);
        }

        /// <summary>
        /// Carga los datos del item actual desde la caché o desde el CSV si es necesario.
        /// </summary>
        private void CargarDatosDelItem()
        {
            CargarItemsSiHaceFalta();

            if (Objeto.ItemsCache.TryGetValue(_itemId, out ItemData datos))
            {
                _itemName = datos.Name;
                _animationPath = datos.AnimationPath;
                _itemDescription = datos.Description;
                _itemHistory = datos.History;
                return;
            }
            else
            {
                GD.PushWarning($"Objeto: no se encontró el item con id {_itemId} en {ItemsCsvPath}.");
            }
        }

        /// <summary>
        /// Carga en memoria todos los items del CSV la primera vez que se necesitan.
        /// </summary>
        private static void CargarItemsSiHaceFalta()
        {
            if (_itemsCargados && ItemsCache.Count > 0)
            {
                return;
            }

            ItemsCache.Clear();

            var file = FileAccess.Open(ItemsCsvPath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PushError($"Objeto: no se pudo abrir {ItemsCsvPath}.");
                return;
            }

            using (file)
            {
                while (!file.EofReached())
                {
                    var row = file.GetCsvLine();

                    if (row.Length == 0)
                    {
                        continue;
                    }

                    string idTexto = row[0].StripEdges().TrimStart('\uFEFF');
                    if (string.IsNullOrEmpty(idTexto) || idTexto.Equals("id", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (row.Length < 5)
                    {
                        GD.PushWarning($"Objeto: fila incompleta en {ItemsCsvPath} para id '{idTexto}'.");
                        continue;
                    }

                    if (!int.TryParse(idTexto, out int id))
                    {
                        GD.PushWarning($"Objeto: id inválida '{idTexto}' en {ItemsCsvPath}.");
                        continue;
                    }

                    ItemsCache[id] = new ItemData(
                        row[1].StripEdges(),
                        row[2].StripEdges(),
                        row[3].StripEdges(),
                        row[4].StripEdges()
                    );
                }
            }

            _itemsCargados = ItemsCache.Count > 0;
        }

        /// <summary>
        /// Representa los datos de un item cargados desde el CSV.
        /// </summary>
        private sealed class ItemData
        {
            /// <summary>Nombre del item.</summary>
            public string Name { get; }

            /// <summary>Ruta del recurso visual del item.</summary>
            public string AnimationPath { get; }

            /// <summary>Descripción del item.</summary>
            public string Description { get; }

            /// <summary>Historia o lore del item.</summary>
            public string History { get; }

            /// <summary>
            /// Crea una nueva instancia con los datos del item.
            /// </summary>
            /// <param name="name">Nombre del item.</param>
            /// <param name="animationPath">Ruta del recurso visual del item.</param>
            /// <param name="description">Descripción del item.</param>
            /// <param name="history">Historia del item.</param>
            public ItemData(string name, string animationPath, string description, string history)
            {
                Name = name;
                AnimationPath = animationPath;
                Description = description;
                History = history;
            }
        }

        /// <summary>
        /// Recoge el objeto y otorga la habilidad correspondiente según su id.
        /// </summary>
        /// <param name="player">Jugador que recoge el objeto.</param>
        public void Recoger(Lira player)
        {
            if (_itemId == 0) player.GiveDoubleJump();
            else if (_itemId == 1) player.GiveDash();
            player._objectoDescription.ChangeVisibility(true, _itemId);
            GetTree().Paused = true;
            Visible = false;
            CallDeferred(MethodName.QueueFree);
        }

        /// <summary>
        /// Devuelve el identificador del item asociado a este objeto.
        /// </summary>
        /// <returns>Id del item.</returns>
        public int GetItemId() => _itemId;

        /// <summary>
        /// Devuelve la posición mundial del objeto para usarla como punto de recogida.
        /// </summary>
        /// <returns>Posición mundial del objeto.</returns>
        public Vector2 GetPickUpPosition() => GlobalPosition;
    }
}

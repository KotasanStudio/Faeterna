using Faeterna.Scripts.Personaje;
using Godot;
using System;
using System.Collections.Generic;

namespace Faeterna.scripts.Mapa
{
    public partial class Objeto : Node2D
    {
        private const string ItemsCsvPath = "res://data/items.csv";

        [Export] private int _itemId;
        [Export] private bool _isTutorialItem = false;
        [Export] private Node2D _tutorial;

        [ExportGroup("Datos del ítem")]
        [Export] private string _itemName;
        [Export] private string _animationPath;
        [Export] private string _itemDescription;
        [Export] private string _itemHistory;

        [ExportGroup("Animación de Aro de Luz")]
        [Export] private PointLight2D _aroLuz;
        [Export] private float _escalaMinima = 0.1f;
        [Export] private float _escalaMaxima = 1.0f;
        [Export] private float _duracionAnimacion = 2.0f;

        private float _tiempoTranscurrido;
        private static readonly Dictionary<int, ItemData> ItemsCache = new();
        private static bool _itemsCargados;

        public string ItemName => _itemName;
        public string AnimationPath => _animationPath;
        public string ItemDescription => _itemDescription;
        public string ItemHistory => _itemHistory;

        public override void _Ready()
        {
            CargarDatosDelItem();

            if (_aroLuz != null)
            {
                _aroLuz.TextureScale = _escalaMinima;
            }

            _tutorial.Visible = _isTutorialItem;
        }

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

        private void CargarDatosDelItem()
        {
            CargarItemsSiHaceFalta();

            if (ItemsCache.TryGetValue(_itemId, out ItemData datos))
            {
                _itemName = datos.Name;
                _animationPath = datos.AnimationPath;
                _itemDescription = datos.Description;
                _itemHistory = datos.History;
                return;
            }

            GD.PushWarning($"Objeto: no se encontró el item con id {_itemId} en {ItemsCsvPath}.");
        }

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
        private sealed class ItemData
        {
            public string Name { get; }
            public string AnimationPath { get; }
            public string Description { get; }
            public string History { get; }

            public ItemData(string name, string animationPath, string description, string history)
            {
                Name = name;
                AnimationPath = animationPath;
                Description = description;
                History = history;
            }
        }
        public void Recoger(Lira player)
        {
            if (_itemId == 0) player.GiveDoubleJump();
            else if (_itemId == 1) player.GiveDash();
            player._objectoDescription.ChangeVisibility(true, _itemId);
            GetTree().Paused = true;
            Visible = false;
            CallDeferred(MethodName.QueueFree);
        }
        public int GetItemId() => _itemId;

        public Vector2 GetPickUpPosition() => GlobalPosition;
    }
}

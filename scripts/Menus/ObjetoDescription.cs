using Godot;
using System.Collections.Generic;

namespace Faeterna.Scripts.Menus
{
    public partial class ObjetoDescription : Control
    {
        private const string ItemsCsvPath = "res://data/items.csv";

        [Export] private int _itemId;
        [Export] private TextureRect _imageAbility;
        [Export] private Label _nameAbility;
        [Export] private Label _descriptionAbility;
        [Export] private Label _historyAbility;

        private static readonly Dictionary<int, ItemData> ItemsCache = new();
        private static bool _itemsCargados;

    public override void _Ready()
    {
      CargarDatosDelItem();
    }

    private void CargarDatosDelItem()
    {
      CargarItemsSiHaceFalta();

      if (!ItemsCache.TryGetValue(_itemId, out ItemData datos))
      {
        GD.PushWarning($"ObjetoDescription: no se encontró el item con id {_itemId} en {ItemsCsvPath}.");
        return;
      }

      if (_nameAbility != null)
      {
        _textSet(_nameAbility, datos.Name);
      }

      if (_descriptionAbility != null)
      {
        _textSet(_descriptionAbility, datos.Description);
      }

      if (_historyAbility != null)
      {
        _textSet(_historyAbility, datos.History);
      }

      if (_imageAbility != null)
      {
        AplicarImagen(datos.AnimationPath);
      }
    }

    private void AplicarImagen(string resourcePath)
    {
      if (string.IsNullOrWhiteSpace(resourcePath))
      {
        _imageAbility.Texture = null;
        return;
      }

      Resource recurso = ResourceLoader.Load(resourcePath);
      if (recurso is Texture2D textura)
      {
        _imageAbility.Texture = textura;
        return;
      }

      GD.PushWarning($"ObjetoDescription: el recurso '{resourcePath}' no es una textura válida para la id {_itemId}.");
    }

    private static void CargarItemsSiHaceFalta()
    {
      if (_itemsCargados)
      {
        return;
      }

      _itemsCargados = true;

      using FileAccess file = FileAccess.Open(ItemsCsvPath, FileAccess.ModeFlags.Read);
      if (file == null)
      {
        GD.PushError($"ObjetoDescription: no se pudo abrir {ItemsCsvPath}.");
        return;
      }

      while (!file.EofReached())
      {
        var row = file.GetCsvLine();

        if (row.Length == 0)
        {
          continue;
        }

        string idTexto = row[0].StripEdges();
        if (string.IsNullOrEmpty(idTexto) || idTexto == "id")
        {
          continue;
        }

        if (row.Length < 5)
        {
          GD.PushWarning($"ObjetoDescription: fila incompleta en {ItemsCsvPath} para id '{idTexto}'.");
          continue;
        }

        if (!int.TryParse(idTexto, out int id))
        {
          GD.PushWarning($"ObjetoDescription: id inválida '{idTexto}' en {ItemsCsvPath}.");
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

    private static void _textSet(Label label, string value)
    {
      label.Text = value;
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
    }
}


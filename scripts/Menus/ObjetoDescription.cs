using Godot;
using System;
using System.Collections.Generic;

namespace Faeterna.Scripts.Menus
{
    public partial class ObjetoDescription : Control
    {
        private const string ItemsCsvPath = "res://data/items.csv";
        private const string ImageNodePath = "MarginContainer/VBoxContainer/TextureRect2/MarginContainer/HBoxContainer/TextureRect";
        private const string NameNodePath = "MarginContainer/VBoxContainer/TextureRect2/MarginContainer/HBoxContainer/Name";
        private const string DescriptionNodePath = "MarginContainer/VBoxContainer/TextureRect2/MarginContainer/HBoxContainer/Description";
        private const string HistoryNodePath = "MarginContainer/VBoxContainer/TextureRect2/MarginContainer/HBoxContainer/Lore";

        [Export] private int _itemId;
        [Export] private TextureRect _imageAbility;
        [Export] private Label _nameAbility;
        [Export] private Label _descriptionAbility;
        [Export] private Label _historyAbility;
        [Signal] public delegate void MenuClosedEventHandler();
        private static readonly Dictionary<int, ItemData> ItemsCache = new();
        private static bool _itemsCargados;

    public override void _Ready()
    {
      EnlazarNodosSiHaceFalta();
      CargarDatosDelItem();
    }

    private void EnlazarNodosSiHaceFalta()
    {
      _imageAbility ??= GetNodeOrNull<TextureRect>(ImageNodePath);
      _nameAbility ??= GetNodeOrNull<Label>(NameNodePath);
      _descriptionAbility ??= GetNodeOrNull<Label>(DescriptionNodePath);
      _historyAbility ??= GetNodeOrNull<Label>(HistoryNodePath);

      if (_nameAbility == null || _descriptionAbility == null || _historyAbility == null || _imageAbility == null)
      {
        GD.PushWarning("ObjetoDescription: faltan referencias de UI. Revisa rutas de nodos o exports del inspector.");
      }
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
      if (_itemsCargados && ItemsCache.Count > 0)
      {
        return;
      }

      ItemsCache.Clear();

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

        string idTexto = row[0].StripEdges().TrimStart('\uFEFF');
        if (string.IsNullOrEmpty(idTexto) || idTexto.Equals("id", StringComparison.OrdinalIgnoreCase))
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

      _itemsCargados = ItemsCache.Count > 0;
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
    public void ChangeVisibility(bool visible, int itemId)
    {
        Visible = visible;
        if (visible)
        {
            EnlazarNodosSiHaceFalta();
            _itemId = itemId;
            CargarDatosDelItem();
        }
    }
    public override void _Input(InputEvent ev)
    {
        // Solo si el menú es visible y presionan una tecla
        if (Visible && ev is InputEventKey { Pressed: true })
        {
            ChangeVisibility(false, _itemId);
            GetTree().Paused = false;
            EmitSignal(SignalName.MenuClosed); // <--- Avisamos que terminamos
        }
    }
  }

}


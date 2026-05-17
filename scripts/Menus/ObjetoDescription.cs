using Godot;
using System;
using System.Collections.Generic;

namespace Faeterna.Scripts.Menus
{
  /// <summary>
  /// Ventana de descripción de objetos y habilidades. Carga la información desde un CSV
  /// y muestra nombre, descripción, historia e imagen del item seleccionado.
  /// </summary>
  public partial class ObjetoDescription : Control
  {
    /// <summary>
    /// Ruta al archivo CSV con los datos de los items.
    /// </summary>
    private const string ItemsCsvPath = "res://data/items.csv";

    /// <summary>
    /// Ruta del nodo de la imagen del item dentro de la escena.
    /// </summary>
    private const string ImageNodePath = "MarginContainer/VBoxContainer/TextureRect2/MarginContainer/HBoxContainer/TextureRect";

    /// <summary>
    /// Ruta del nodo que muestra el nombre del item.
    /// </summary>
    private const string NameNodePath = "MarginContainer/VBoxContainer/TextureRect2/MarginContainer/HBoxContainer/Name";

    /// <summary>
    /// Ruta del nodo que muestra la descripción del item.
    /// </summary>
    private const string DescriptionNodePath = "MarginContainer/VBoxContainer/TextureRect2/MarginContainer/HBoxContainer/Description";

    /// <summary>
    /// Ruta del nodo que muestra la historia o lore del item.
    /// </summary>
    private const string HistoryNodePath = "MarginContainer/VBoxContainer/TextureRect2/MarginContainer/HBoxContainer/Lore";

    /// <summary>
    /// Identificador del item que se mostrará en el menú.
    /// </summary>
    [Export] private int _itemId;

    /// <summary>
    /// Imagen asociada al item seleccionado.
    /// </summary>
    [Export] private TextureRect _imageAbility;

    /// <summary>
    /// Label donde se muestra el nombre del item.
    /// </summary>
    [Export] private Label _nameAbility;

    /// <summary>
    /// Label donde se muestra la descripción del item.
    /// </summary>
    [Export] private Label _descriptionAbility;

    /// <summary>
    /// Label donde se muestra la historia del item.
    /// </summary>
    [Export] private Label _historyAbility;

    /// <summary>
    /// Señal emitida cuando el menú de descripción se cierra.
    /// </summary>
    [Signal] public delegate void MenuClosedEventHandler();

    /// <summary>
    /// Caché estática con los datos de items cargados desde el CSV.
    /// </summary>
    private static readonly Dictionary<int, ItemData> ItemsCache = new();

    /// <summary>
    /// Indica si la caché ya fue cargada al menos una vez.
    /// </summary>
    private static bool _itemsCargados;

    private Timer _wait;
    private bool _canBeSkip = false;

    /// <summary>
    /// Inicializa el nodo, enlaza referencias si faltan y carga el item correspondiente.
    /// </summary>
    public override void _Ready()
    {
      EnlazarNodosSiHaceFalta();
      CargarDatosDelItem();

      _wait = new Timer { WaitTime = 1f, OneShot = true };
      AddChild(_wait);
      _wait.Timeout += () => { _canBeSkip = true; };
    }

    /// <summary>
    /// Intenta enlazar los nodos de interfaz si no están asignados desde el inspector.
    /// </summary>
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

    /// <summary>
    /// Carga y muestra los datos del item actual.
    /// </summary>
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

    /// <summary>
    /// Aplica al control de imagen el recurso indicado si es una textura válida.
    /// </summary>
    /// <param name="resourcePath">Ruta del recurso a cargar.</param>
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

    /// <summary>
    /// Carga los items desde el CSV si todavía no están en memoria.
    /// </summary>
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

    /// <summary>
    /// Asigna el texto de un Label de forma centralizada.
    /// </summary>
    /// <param name="label">Label a modificar.</param>
    /// <param name="value">Texto que se quiere mostrar.</param>
    private static void _textSet(Label label, string value)
    {
      label.Text = value;
    }

    /// <summary>
    /// Estructura interna con los datos cargados de cada item del CSV.
    /// </summary>
    private sealed class ItemData
    {
      /// <summary>Nombre visible del item.</summary>
      public string Name { get; }

      /// <summary>Ruta del recurso gráfico/animación del item.</summary>
      public string AnimationPath { get; }

      /// <summary>Descripción funcional del item.</summary>
      public string Description { get; }

      /// <summary>Lore o historia del item.</summary>
      public string History { get; }

      /// <summary>
      /// Crea una nueva entrada de item con sus textos y recurso asociado.
      /// </summary>
      /// <param name="name">Nombre del item.</param>
      /// <param name="animationPath">Ruta del recurso visual del item.</param>
      /// <param name="description">Descripción del item.</param>
      /// <param name="history">Historia o lore del item.</param>
      public ItemData(string name, string animationPath, string description, string history)
      {
        Name = name;
        AnimationPath = animationPath;
        Description = description;
        History = history;
      }
    }

    /// <summary>
    /// Muestra u oculta la ventana de descripción y, si se abre, carga el item indicado.
    /// </summary>
    /// <param name="visible">Indica si la ventana debe mostrarse.</param>
    /// <param name="itemId">Identificador del item a visualizar.</param>
    public void ChangeVisibility(bool visible, int itemId)
    {
      Visible = visible;
      if (visible)
      {
        EnlazarNodosSiHaceFalta();
        _itemId = itemId;
        _canBeSkip = false;
        CargarDatosDelItem();
        _wait?.Start();
      }
    }

    /// <summary>
    /// Cierra la ventana cuando el jugador pulsa una tecla mientras está visible.
    /// </summary>
    /// <param name="ev">Evento de entrada recibido por el nodo.</param>
    public override void _Input(InputEvent ev)
    {
      // Solo si el menú es visible y presionan una tecla
      if (Visible && ev is InputEventKey { Pressed: true } && _canBeSkip)
      {
        ChangeVisibility(false, _itemId);
        GetTree().Paused = false;
        EmitSignal(SignalName.MenuClosed);
      }
    }
    }
}


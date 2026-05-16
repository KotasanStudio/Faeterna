using Godot;
using System;
using System.Collections.Generic;

namespace Faeterna.scripts.Personaje
{
    public partial class TerrainParticles : GpuParticles2D
    {
        /// <summary>RayCast2D que se usará para detectar el tile debajo del personaje y cambiar el atlas de partículas según la "RutaParticula" definida en el TileData.</summary>
	    [Export] private RayCast2D _rayCast;

        /// <summary>Atlas de partículas para la id 1 (por ejemplo: hierba).</summary>
        [Export] private AtlasTexture _atlasTextureId1;

        /// <summary>Atlas de partículas para la id 2 (otra ruta/sprites distintos).</summary>
        [Export] private AtlasTexture _atlasTextureId2;

        /// <summary>Tamaño en pixeles de cada frame del spritesheet</summary>
        [Export] private Vector2I _frameSize = new(8, 8);

        /// <summary>Si es true, cada partícula se queda en el frame aleatorio con el que nace.</summary>
        [Export] private bool _freezeRandomFrameOnSpawn = true;

        /// <summary>Mapa de RutaParticula -> atlas que se debe usar.</summary>
        private readonly Dictionary<int, AtlasTexture> _sourceAtlases = new();

        /// <summary>Copias en runtime para poder cambiar la región sin alterar el recurso compartido.</summary>
        private readonly Dictionary<int, AtlasTexture> _runtimeAtlases = new();

        /// <summary>Id del atlas actualmente asignado a la propiedad Texture. Se usa para evitar reasignar el mismo atlas y resetear la animación de partículas innecesariamente.</summary>
        private int _currentTextureId = -1;

        /// <summary>
        /// En _Ready se configuran los atlas de partículas a usar según lo asignado en el inspector y se hacen copias para modificar en runtime sin afectar los recursos originales. También se configura el material de proceso para que cada partícula elija un frame aleatorio al nacer, y se asigna el atlas inicial (id 1) a la propiedad Texture.
        /// </summary>
        public override void _Ready()
        {
            _sourceAtlases.Clear();
            _runtimeAtlases.Clear();

            if (_atlasTextureId1 != null)
                _sourceAtlases[1] = _atlasTextureId1;

            if (_atlasTextureId2 != null)
                _sourceAtlases[2] = _atlasTextureId2;

            if (!_sourceAtlases.ContainsKey(1))
            {
                var fallbackAtlas = Texture as AtlasTexture;
                if (fallbackAtlas != null)
                    _sourceAtlases[1] = fallbackAtlas;
            }

            if (!_sourceAtlases.ContainsKey(1))
            {
                GD.PushWarning("TerrainParticles necesita un AtlasTexture para la id 1 asignado en el inspector.");
                return;
            }

            if (!_sourceAtlases.ContainsKey(2))
            {
                GD.PushWarning("TerrainParticles no tiene atlas asignado para la id 2; se reutilizará el de la id 1 hasta que pongas otro.");
                _sourceAtlases[2] = _sourceAtlases[1];
            }

            foreach (var pair in _sourceAtlases)
            {
                var runtimeAtlas = (AtlasTexture)pair.Value.Duplicate();
                _runtimeAtlases[pair.Key] = runtimeAtlas;
            }

            ConfigureProcessMaterialRandomFrame();

            ApplySpriteSheetLayout(_runtimeAtlases[1]);
            Texture = _runtimeAtlases[1];
            _currentTextureId = 1;
        }

        /// <summary>
        /// En cada frame, el método verifica si el RayCast2D está colisionando con un TileMapLayer. Si es así, obtiene las coordenadas del tile debajo del personaje y revisa si tiene datos personalizados para "RutaParticula". Si encuentra una ruta válida, cambia el atlas de partículas a la correspondiente a esa ruta. Si la ruta no es válida o no hay colisión, mantiene el atlas actual.
        /// </summary>
        /// <param name="delta">
        /// Tiempo en segundos desde el último frame. No se usa directamente en este método, pero es parte de la firma requerida por Godot para el método _Process.
        /// </param>
        public override void _Process(double delta)
        {
            if (_rayCast == null || !_rayCast.IsColliding())
                return;

            var collider = _rayCast.GetCollider();
            if (collider is not TileMapLayer tileMapLayer)
                return;

            var localCollisionPoint = tileMapLayer.ToLocal(_rayCast.GetCollisionPoint());
            var cellCoords = tileMapLayer.LocalToMap(localCollisionPoint);
            var tileData = tileMapLayer.GetCellTileData(cellCoords);

            if (tileData == null)
                return;

            var textureId = tileData.GetCustomData("RutaParticula").AsInt32();
            if (textureId <= 0)
                return;

            if (!_runtimeAtlases.TryGetValue(textureId, out var runtimeAtlas))
            {
                if (!_runtimeAtlases.TryGetValue(1, out runtimeAtlas))
                    return;

                textureId = 1;
            }

            if (_currentTextureId == textureId)
                return;

            ApplySpriteSheetLayout(runtimeAtlas);
            Texture = runtimeAtlas;
            _currentTextureId = textureId;
        }

        /// <summary>
        /// Calcula y aplica el número de frames horizontales y verticales a usar en el shader de partículas según el tamaño del frame definido y el tamaño del atlas. Si el atlas no es válido o el tamaño del frame no es positivo, se asigna 1 frame en ambas direcciones para evitar divisiones por cero o comportamientos inesperados.
        /// </summary>
        /// <param name="atlasTexture">
        /// El AtlasTexture del cual se quiere calcular el layout de frames. Se espera que este atlas ya tenga asignada la región correcta si es que se usan regiones dentro de un atlas más grande.
        /// </param>
        private void ApplySpriteSheetLayout(AtlasTexture atlasTexture)
        {
            if (atlasTexture?.Atlas == null || _frameSize.X <= 0 || _frameSize.Y <= 0)
            {
                Set("h_frames", 1);
                Set("v_frames", 1);
                return;
            }

            var regionSize = atlasTexture.Region.Size;
            var textureSize = regionSize.X > 0 && regionSize.Y > 0
                ? regionSize
                : atlasTexture.Atlas.GetSize();

            var hFrames = Math.Max(1, (int)textureSize.X / _frameSize.X);
            var vFrames = Math.Max(1, (int)textureSize.Y / _frameSize.Y);

            Set("h_frames", hFrames);
            Set("v_frames", vFrames);
        }

        /// <summary>
        /// Configura el material de proceso para que cada partícula elija un frame aleatorio al nacer. Esto se logra asignando un rango de offset de animación entre 0 y 1, lo que hace que el shader escoja un punto aleatorio en la animación para cada partícula. Si la opción _freezeRandomFrameOnSpawn está activada, también se establece la velocidad de animación a 0 para que las partículas no cambien de frame después de nacer.
        /// </summary>
        private void ConfigureProcessMaterialRandomFrame()
        {
            if (ProcessMaterial is not ParticleProcessMaterial processMaterial)
            {
                GD.PushWarning("TerrainParticles necesita un ParticleProcessMaterial para randomizar frames por partícula.");
                return;
            }

            processMaterial.Set("anim_offset_min", 0.0f);
            processMaterial.Set("anim_offset_max", 1.0f);

            if (_freezeRandomFrameOnSpawn)
            {
                processMaterial.Set("anim_speed_min", 0.0f);
                processMaterial.Set("anim_speed_max", 0.0f);
            }
        }
    }
}


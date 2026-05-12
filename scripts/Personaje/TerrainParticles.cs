using Godot;
using System;
using System.Collections.Generic;

namespace Faeterna.scripts.Personaje
{
    public partial class TerrainParticles : GpuParticles2D
    {
	    [Export] private RayCast2D _rayCast;

        /// <summary>Atlas de partículas para la id 1 (por ejemplo: hierba).</summary>
        [Export] private AtlasTexture _atlasTextureId1;

        /// <summary>Atlas de partículas para la id 2 (otra ruta/sprites distintos).</summary>
        [Export] private AtlasTexture _atlasTextureId2;

        /// <summary>
        /// Tamaño en pixeles de cada frame del spritesheet.
        /// En tu caso: 8x8.
        /// </summary>
        [Export] private Vector2I _frameSize = new(8, 8);

        /// <summary>
        /// Si es true, cada partícula se queda en el frame aleatorio con el que nace.
        /// </summary>
        [Export] private bool _freezeRandomFrameOnSpawn = true;

        /// <summary>
        /// Mapa de RutaParticula -> atlas que se debe usar.
        /// </summary>
        private readonly Dictionary<int, AtlasTexture> _sourceAtlases = new();

        /// <summary>
        /// Copias en runtime para poder cambiar la región sin alterar el recurso compartido.
        /// </summary>
        private readonly Dictionary<int, AtlasTexture> _runtimeAtlases = new();

        private int _currentTextureId = -1;

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

            // Hace que cada partícula elija un frame distinto al nacer.
            ConfigureProcessMaterialRandomFrame();

            ApplySpriteSheetLayout(_runtimeAtlases[1]);
            Texture = _runtimeAtlases[1];
            _currentTextureId = 1;
        }

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


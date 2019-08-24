using UnityEngine;

namespace Assets.World
{
    /// <summary>
    /// This class helps you handle communication with the GPU by exposing human-friendly interface.
    /// Just change flags the cells you want to be highlighted to true and voilà.
    /// Important: To actually apply the data to the texture and push it to the GPU, SendDataToGPU method need to be called in LateUpdate.
    /// </summary>
    sealed class GridShaderAdapter
    {
        const byte SELECTED_CELL_INDICATOR = 200; // this number is arbitrary - grid shader recognizes everything > 0.5 as selected

        Texture2D _cellTexture;
        Color32[] _cellTextureData;

        internal bool this[Vector2Int coord]
        {
            get => _cellTextureData[coord.y * GameMap.GridSizeY + coord.x].r == SELECTED_CELL_INDICATOR;
            set => _cellTextureData[coord.y * GameMap.GridSizeY + coord.x]
                = new Color32(value ? SELECTED_CELL_INDICATOR : (byte)0, 0, 0, 0);
        }

        internal void InitializeCellTexture()
        {
            if (_cellTexture)
            {
                _cellTexture.Resize(GameMap.GridSizeX, GameMap.GridSizeY); // we don't need to check if the precious size was the same
            }
            else
            {
                // each pixel corresponds to one cell
                _cellTexture = new Texture2D(GameMap.GridSizeX, GameMap.GridSizeY, TextureFormat.RGBA32, false, true)
                {
                    filterMode = FilterMode.Point, // we don't want to blend cell data, so we use point filtering
                    wrapModeU = TextureWrapMode.Clamp, // the data shouldn't wrap
                    wrapModeV = TextureWrapMode.Clamp  // the data shouldn't wrap
                };

                // make it globally known as _CellData
                Shader.SetGlobalTexture("_CellData", _cellTexture);
            }

            if (_cellTextureData == null || _cellTextureData.Length != GameMap.GridSizeX * GameMap.GridSizeY)
                _cellTextureData = new Color32[GameMap.GridSizeX * GameMap.GridSizeY];
        }

        internal void SendDataToGPU()
        {
            // To actually apply the data to the texture and push it to the GPU, 
            // we have to invoke Texture2D.SetPixels32 followed by Texture2D.Apply.
            _cellTexture.SetPixels32(_cellTextureData);
            _cellTexture.Apply();
        }

        internal void ResetAllSelection()
        {
            for (int i = 0; i < _cellTextureData.Length; i++)
                _cellTextureData[i] = new Color32(0, 0, 0, 0);
        }
    }
}

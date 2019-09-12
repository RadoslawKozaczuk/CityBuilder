using System.Collections.Generic;
using UnityEngine;

namespace Assets.World
{
    /// <summary>
    /// This class helps you handle communication with the GPU by exposing human-friendly interface.
    /// Just set the flags of the cells you want to highlight to true and voilà.
    /// You can access the flags just like you would normally access an array.
    /// Important: To actually apply the data, call SendDataToGPU method (preferably at the end of a frame).
    /// </summary>
    sealed class GridShaderAdapter
    {
        const byte SELECTED_CELL_INDICATOR = 200; // this number is arbitrary - grid shader recognizes everything >0.5f (>127) as selected

        // custom indexer to allow convenient access
        internal bool this[Vector2Int coord]
        {
            get => _cellTextureData[coord.y * GameMap.GridSizeY + coord.x].r == SELECTED_CELL_INDICATOR;
            set
            {
                _cellTextureData[coord.y * GameMap.GridSizeY + coord.x] = new Color32(value ? SELECTED_CELL_INDICATOR : (byte)0, 0, 0, 0);
                _isDirty = true;
            }
        }

        readonly Texture2D _cellTexture;
        readonly Color32[] _cellTextureData;
        bool _isDirty = true; // to prevent from redundant calls, initially set to true to reset the shader

        internal GridShaderAdapter()
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

            _cellTextureData = new Color32[GameMap.GridSizeX * GameMap.GridSizeY];

            SendDataToGPU();
        }

        internal void SetData(List<Vector2Int> data, bool resetPreviousData = false)
        {
#if UNITY_EDITOR
            if (data == null)
                throw new System.ArgumentNullException("data", "Data send to GridShaderAdapter is null. "
                    + "If you intended to reset the selection use ResetAllSelection method instead.");
            else if (data.Count == 0)
                throw new System.ArgumentException("Data send to GridShaderAdapter is empty." 
                    + "If you intended to reset the selection use ResetAllSelection method instead.");
#endif

            if (resetPreviousData)
                ResetAllSelection();

            foreach (Vector2Int v in data)
                this[v] = true;
        }

        internal void SendDataToGPU()
        {
            if (!_isDirty)
                return; // nothing changed, no reason to bother the GPU

            // To actually apply the data to the texture and push it to the GPU, 
            // we have to invoke Texture2D.SetPixels32 followed by Texture2D.Apply.
            _cellTexture.SetPixels32(_cellTextureData);
            _cellTexture.Apply();

            _isDirty = false;
        }

        internal void ResetAllSelection()
        {
            for (int i = 0; i < _cellTextureData.Length; i++)
                _cellTextureData[i] = new Color32(0, 0, 0, 0);

            _isDirty = true;
        }
    }
}

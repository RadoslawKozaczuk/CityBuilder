using UnityEngine;

namespace Assets.Scripts
{
    public class Grid : MonoBehaviour
    {
        public GridCell[,] Cells;

        [SerializeField] int _gridSizeX = 12, _gridSizeY = 12;

        // to allow designers to put the plane in an arbitrary position in the world space
        float _gridOffsetX, _gridOffsetZ;

        void Start()
        {
            Cells = new GridCell[_gridSizeX, _gridSizeY];
            for (int i = 0; i < _gridSizeX; i++)
                for (int j = 0; j < _gridSizeY; j++)
                    Cells[i, j] = new GridCell() { X = i, Y = j };

            _gridOffsetX = transform.position.x;
            _gridOffsetZ = transform.position.z;
        }

        public bool GetCell(Ray ray, out GridCell cell)
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                cell = GetCell(hit.point);
                return true;
            }

            cell = new GridCell(); // dummy
            return false;
        }

        /// <summary>
        /// This returns left bottom corner of the cell
        /// </summary>
        public Vector3 GetCellPosition(GridCell cell)
        {
            Vector3 pos = transform.position;
            pos.x += cell.X * 10f - _gridOffsetX;
            pos.z += cell.Y * 10f + _gridOffsetZ;

            return pos;
        }

        /// <summary>
        /// Checks if there is a free area of the given size under the given cell. 
        /// X and y are at the bottom (perspective camera).
        /// </summary>
        public bool IsAreaFree(int x, int y, int sizeX, int sizeY)
        {
            if (x + sizeX > _gridSizeX || y - sizeY < -1) // y=2 sizeY=3 => still should be possible
                return false;

            for (int i = x; i < x + sizeX; i++)
                for (int j = y; j > y - sizeY; j--)
                    if (Cells[i, j].IsOccupied)
                        return false;

            return true;
        }

        /// <summary>
        /// Checks if there is a free area of the given size under the given cell. 
        /// X and y are at the bottom (perspective camera).
        /// Additional parameter allow us to exclude certain building.
        /// </summary>
        public bool IsAreaFree(int x, int y, int sizeX, int sizeY, Building exclude)
        {
            if (x + sizeX > _gridSizeX || y - sizeY < -1) // y=2 sizeY=3 => still should be possible
                return false;

            for (int i = x; i < x + sizeX; i++)
                for (int j = y; j > y - sizeY; j--)
                    if (Cells[i, j].IsOccupied && Cells[i, j].Building != exclude)
                        return false;

            return true;
        }

        /// <summary>
        /// Mark all the cells in the given area as occupied.
        /// </summary>
        public void MarkAreaAsOccupied(int x, int y, int sizeX, int sizeY, Building building)
        {
            if (x + sizeX > _gridSizeX || y - sizeY < -1) // y=2 sizeY=3 => still should be possible
                return;

            for (int i = x; i < x + sizeX; i++)
                for (int j = y; j > y - sizeY; j--)
                    Cells[i, j].Building = building;
        }

        /// <summary>
        /// Mark all the cells in the given area as free.
        /// </summary>
        public void MarkAreaAsFree(int x, int y, int sizeX, int sizeY)
        {
            if (x + sizeX > _gridSizeX || y - sizeY < -1) // y=2 sizeY=3 => still should be possible
                return;

            for (int i = x; i < x + sizeX; i++)
                for (int j = y; j > y - sizeY; j--)
                    Cells[i, j].Building = null;
        }

        // Get cell returns cell from a given position
        GridCell GetCell(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);

            int coordX = (int)Mathf.Floor(Map(0, 12, -5, 5, position.x));
            int coordY = (int)Mathf.Floor(Map(0, 12, -5, 5, position.z));
            Debug.Log("CellCoord: x: " + coordX + ", z: " + coordY);

            return Cells[coordX, coordY];
        }

        /// <summary>
        /// Map value from one range to another.
        /// </summary>
        float Map(float newmin, float newmax, float origmin, float origmax, float value) =>
            Mathf.Lerp(newmin, newmax, Mathf.InverseLerp(origmin, origmax, value));
    }
}

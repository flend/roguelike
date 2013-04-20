using System;
using System.Collections.Generic;
using System.Text;

namespace TileEngine
{
    class TileRow
    {
        List<TileCell> columns;

        /// <summary>
        /// Build a row of numColumn columns. Current implementation is non-resizable, so out-of-bounds access throws an exception
        /// </summary>
        /// <param name="numColumns"></param>
        public TileRow(int numColumns)
        {
            columns = new List<TileCell>(numColumns);

            for (int i = 0; i < numColumns; i++)
            {
                columns.Add(new TileCell());
            }
        }

        public List<TileCell> Columns
        {
            get
            {
                return columns;
            }
        }

        /// <summary>
        /// Indexer
        /// </summary>
        public TileCell this[int index]
        {

            get
            {
                return columns[index];
            }
            set
            {
                columns[index] = value;
            }
        }
    }

    class TileLayer
    {
        List<TileRow> rows;

        /// <summary>
        /// Build a layer of numRows x numColumn cells. Current implementation is non-resizable, so out-of-bounds access throws an exception
        /// </summary>
        public TileLayer(int numRows, int numColumns)
        {
            rows = new List<TileRow>();

            for (int i = 0; i < numRows; i++)
            {
                rows.Add(new TileRow(numColumns));
            }
        }

        public List<TileRow> Rows
        {
            get
            {
                return rows;
            }
        }

        /// <summary>
        /// Indexer
        /// </summary>
        public TileRow this[int index]
        {

            get
            {
                return rows[index];
            }
            set
            {
                rows[index] = value;
            }
        }
    }

    /// <summary>
    /// Consists of layers, each layer consisting of the same dimensions.
    /// Each layer is 2d rows and columns
    /// </summary>
    class TileMap
    {
        List<TileLayer> layers;

        int numRows;
        int numColumns;
        int numLayers;

        /// <summary>
        /// Build a map of numLayers layers, each of numRows x numColumns
        /// </summary>
        public TileMap(int numLayers, int numRows, int numColumns)
        {
            this.numLayers = numLayers;
            this.numRows = numRows;
            this.numColumns = numColumns;

            layers = new List<TileLayer>();

            for (int i = 0; i < numLayers; i++)
            {
                layers.Add(new TileLayer(numRows, numColumns));
            }
        }

        public List<TileLayer> Layer
        {
            get
            {
                return layers;
            }
        }

        public int Rows
        {
            get { return numRows; }
        }

        public int Columns
        {
            get { return numColumns; }
        }

        public int Layers
        {
            get { return numLayers; }
        }

        /// <summary>
        /// Indexer
        /// </summary>
        public TileLayer this[int index]
        {

            get
            {
                return layers[index];
            }
            set
            {
                layers[index] = value;
            }
        }
    }
}

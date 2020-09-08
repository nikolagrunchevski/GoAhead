﻿using GoAhead.FPGA;
using GoAhead.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace GoAhead.Commands.GridStyle
{
    class PrintPartitionPinConstraintsForSelection : PrintPartitionPinConstraintsForTile
    {
        private const string MODE_ROW_WISE = "row-wise";
        private const string MODE_COLUMN_WISE = "column-wise";

        private const string HORIZONTAL_LEFT_TO_RIGHT = "left-to-right";
        private const string HORIZONTAL_RIGHT_TO_LEFT = "right-to-left";

        private const string VERTICAL_TOP_DOWN = "top-down";
        private const string VERTICAL_BOTTOM_UP = "bottom-up";

        protected override void DoCommandAction()
        {
            CheckParameters();
            List<Tile> tilesInFinalOrder = GetTilesInFinalOrder(this.Mode, this.Vertical, this.Horizontal);

            int startIndex = IndexOffset;
            int startIndexTemp = 0;

            foreach (Tile t in tilesInFinalOrder)
            {
                if (startIndexTemp >= NumberOfSignals)
                {
                    break;
                }

                SwitchboxName = t.Location;
                StartIndex = startIndex;
                SignalsForTile = MaxSignalsPerTile;
                /*
                if (NumberOfSignals - startIndex < MaxSignalsPerTile)
                {
                    SignalsForTile = NumberOfSignals - startIndex;
                    Console.WriteLine("INFO: There are " + (MaxSignalsPerTile - SignalsForTile) + " signals remaining in " + SwitchboxName);
                }*/

                if (NumberOfSignals - startIndexTemp < MaxSignalsPerTile)
                {
                    SignalsForTile = NumberOfSignals - startIndexTemp;
                    Console.WriteLine("INFO: There are " + (MaxSignalsPerTile - SignalsForTile) + " signals remaining in " + SwitchboxName);
                }

                base.DoCommandAction();

                startIndex += MaxSignalsPerTile;
                startIndexTemp += MaxSignalsPerTile;
            }
        }

        public static List<Tile> GetTilesInFinalOrder(string Mode, string Vertical, string Horizontal)
        {
            List<TileKey> intTiles = new List<TileKey>();

            foreach (Tile t in TileSelectionManager.Instance.GetSelectedTiles().Where(tile =>
                     IdentifierManager.Instance.IsMatch(tile.Location, IdentifierManager.RegexTypes.Interconnect)))
            {
                intTiles.Add(t.TileKey);
            }

            var modeClusters =
                from key in intTiles
                group key by IsRowWise(Mode) ? key.Y : key.X into cluster
                select cluster;

            List<Tile> tilesInFinalOrder = new List<Tile>();

            // order tiles
            if (IsRowWise(Mode))
            {
                foreach (IGrouping<int, TileKey> group in (IsTopDown(Vertical) ? modeClusters.OrderBy(g => g.Key) : modeClusters.OrderByDescending(g => g.Key)))
                {
                    foreach (TileKey key in (IsLeftToRight(Horizontal) ? group.OrderBy(k => k.X) : group.OrderBy(k => k.X).Reverse()))
                    {
                        tilesInFinalOrder.Add(FPGA.FPGA.Instance.GetTile(key));
                    }
                }
            }
            else
            {
                foreach (IGrouping<int, TileKey> group in (IsLeftToRight(Horizontal) ? modeClusters.OrderBy(g => g.Key) : modeClusters.OrderByDescending(g => g.Key)))
                {
                    foreach (TileKey key in (IsTopDown(Vertical) ? group.OrderBy(k => k.Y) : group.OrderBy(k => k.Y).Reverse()))
                    {
                        tilesInFinalOrder.Add(FPGA.FPGA.Instance.GetTile(key));
                    }
                }
            }

            return tilesInFinalOrder;
        }

        private static bool IsRowWise(string Mode)
        {
            return Mode.Equals(MODE_ROW_WISE);
        }

        private static bool IsColumnWise(string Mode)
        {
            return Mode.Equals(MODE_COLUMN_WISE);
        }

        private static bool IsLeftToRight(string Horizontal)
        {
            return Horizontal.Equals(HORIZONTAL_LEFT_TO_RIGHT);
        }
		
        private static bool IsRightToLeft(string Horizontal)
        {
            return Horizontal.Equals(HORIZONTAL_RIGHT_TO_LEFT);
        }
		
        private static bool IsTopDown(string Vertical)
        {
            return Vertical.Equals(VERTICAL_TOP_DOWN);
        }
		
        private static bool IsBottomUp(string Vertical)
        {
            return Vertical.Equals(VERTICAL_BOTTOM_UP);
        }

        private void CheckParameters()
        {
            bool modeIsCorrect = IsRowWise(Mode) || IsColumnWise(Mode);
            bool horizontalIsCorrect = IsLeftToRight(Horizontal) || IsRightToLeft(Horizontal);
            bool verticalIsCorrect = IsTopDown(Vertical) || IsBottomUp(Vertical);
            bool indexOffsetIsCorrect = IndexOffset >= 0;

            if(!modeIsCorrect || !horizontalIsCorrect || !verticalIsCorrect || !indexOffsetIsCorrect)
            {
                throw new ArgumentException("Unexpected format in parameter Mode, Horizontal or/and Vertical.");
            }
        }

        public override void Undo()
        {
            throw new NotImplementedException();
        }

        [Parameter(Comment = "The number of signals.")]
        public int NumberOfSignals = 128;

        [Parameter(Comment = "Either row-wise or column-wise")]
        public string Mode = MODE_ROW_WISE;

        [Parameter(Comment = "Either left-to-right or right-to-left")]
        public string Horizontal = HORIZONTAL_LEFT_TO_RIGHT;

        [Parameter(Comment = "Either top-down or bottom-up")]
        public string Vertical = VERTICAL_TOP_DOWN;

        [Parameter(Comment = "Either top-down or bottom-up")]
        public int IndexOffset = 0;
    }
}

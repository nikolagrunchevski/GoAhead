﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoAhead.FPGA;

namespace GoAhead.Commands.Selection
{
    [CommandDescription(Description = "Select a before saved user selection (see also StoreCurrentSelectionAs)", Wrapper = true)]
    class SelectUserSelection : AddToSelectionCommand
    {
        protected override void DoCommandAction()
        {
            foreach (Tile t in FPGA.TileSelectionManager.Instance.GetAllUserSelectedTiles(this.UserSelectionType))
            {
                // deselect or add the selected tile 
                // in comd umwandeln
                if (FPGA.TileSelectionManager.Instance.IsSelected(t.TileKey))
                {
                    FPGA.TileSelectionManager.Instance.RemoveFromSelection(t.TileKey, false);
                }
                else
                {
                    FPGA.TileSelectionManager.Instance.AddToSelection(t.TileKey, false);
                }

                /*
                AddToSelectionLoc cmd = new AddToSelectionLoc();
                cmd.Notify = false;
                cmd.Location = t.Location;
                CommandExecuter.Instance.Execute(cmd);*/
            }

            FPGA.TileSelectionManager.Instance.SelectionChanged();
        }

        public override void Undo()
        {
            throw new NotImplementedException();
        }

        [Parameter(Comment = "The name of the user selection type")]
        public String UserSelectionType = "PartialArea";
    }
}

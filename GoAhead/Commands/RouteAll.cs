﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoAhead.FPGA;
using GoAhead.Objects;
using GoAhead.Code.XDL;
using GoAhead.Commands.NetlistContainerGeneration;

namespace GoAhead.Commands
{
    [CommandDescription(Description = "Route all unrouted nets", Wrapper = true, Publish = true)]
    class RouteAll : NetlistContainerCommand
    {
        protected override void DoCommandAction()
        {
            NetlistContainer netlistContainer = this.GetNetlistContainer();

            int numberOfUnroutedNets = netlistContainer.Nets.Where(n => n.PipCount == 0 && n.OutpinCount == 1 && n.InpinCount > 0).Count();
            int routedNets = 0;

            List<XDLNet> unrouteableNets = new List<XDLNet>();

            foreach (XDLNet unroutedNet in netlistContainer.Nets.Where(n => n.PipCount == 0 && n.OutpinCount == 1 && n.InpinCount > 0))
            {   
                int pipCount = unroutedNet.PipCount;               

                RouteNet routeCmd = new RouteNet();
                routeCmd.NetlistContainerName = this.NetlistContainerName;
                routeCmd.NetName = unroutedNet.Name;
                routeCmd.SearchMode = this.SearchMode;

                CommandExecuter.Instance.Execute(routeCmd);

                // pip count did not change if no paht was found
                if (pipCount == unroutedNet.PipCount)
                {
                    unrouteableNets.Add(unroutedNet);
                }             
                
                this.ProgressInfo.Progress = this.ProgressStart + (int)((double)routedNets++ / (double)numberOfUnroutedNets * this.ProgressShare);
            }
            
            foreach(XDLNet net in unrouteableNets)
            {
                RouteNet routeCmd = new RouteNet();
                routeCmd.NetlistContainerName = this.NetlistContainerName;
                routeCmd.NetName = net.Name;
                routeCmd.SearchMode = this.SearchMode;

                this.OutputManager.WriteOutput(routeCmd.ToString());

                CommandExecuter.Instance.Execute(routeCmd);
            }
        }

        public override void Undo()
        {
            throw new NotImplementedException();
        }

        [Parameter(Comment = "The path search modue (BFS, DFS)")]
        public String SearchMode = "BFS";
    }
}

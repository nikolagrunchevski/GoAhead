﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GoAhead.Objects;
using GoAhead.Code;
using GoAhead.Code.XDL;
using GoAhead.Commands.NetlistContainerGeneration;

namespace GoAhead.Commands.Debug
{
    [CommandDescription(Description = "Print the expected placement of ports after the mapping phase", Wrapper = false)]
    class PrintExpectedPorts : CommandWithFileOutput
    {
        protected override void DoCommandAction()
        {
            this.OutputManager.WriteOutput("# e.g. expect outpin left_3 SLICE_X5Y83 CMUX static_to_partial 5 -> expect vector index 5 in netname");
            this.OutputManager.WriteOutput("# e.g. expect outpin left_5 SLICE_X342Y283 AMUX static_to_partial x -> no vector");
                 
            foreach (LibElemInst inst in Objects.LibraryElementInstanceManager.Instance.GetAllInstantiations().Where(i => Regex.IsMatch(i.InstanceName, this.InstantiationFilter)))
            {
                LibraryElement libElement = Objects.Library.Instance.GetElement(inst.LibraryElementName);

                this.OutputManager.WriteOutput("# expected ports for instance " + inst.InstanceName + " (instance of " + libElement.Name + ")");

                foreach (XDLPort port in libElement.Containter.Ports)
                {
                    // only consider external ports
                    bool hasKindMapping = inst.PortMapper.HasKindMapping(port.ExternalName);
                    if (hasKindMapping)
                    {
                        PortMapper.MappingKind mappingKind = inst.PortMapper.GetMapping(port.ExternalName);
                        if (mappingKind != PortMapper.MappingKind.External)
                        {
                            continue;
                        }                        
                    }

                    bool hasMapping = inst.PortMapper.HasSignalMapping(port.ExternalName);
                    String inoutPin = port.Direction == FPGA.FPGATypes.PortDirection.In ? "inpin" : "outpin";
                    String portName = hasMapping ? inst.PortMapper.GetSignalName(port.ExternalName) : "unknown";
                    String index = inst.PortMapper.HasIndex(port.ExternalName) ? inst.PortMapper.GetIndex(port.ExternalName).ToString() : "-1";

                    if (portName.Equals("1"))
                    {
                        index = "0";
                    }

                    String line = "expect " + inoutPin + " " + inst.InstanceName + " " + inst.SliceName + " " + port.SlicePort + " " + portName + " " + index;

                    this.OutputManager.WriteOutput(line);
                }                
            }
        }

        public override void Undo()
        {
            throw new NotImplementedException();
        }

        [Parameter(Comment = "Only consider those library element instantiations whose name matches this filter")]
        public String InstantiationFilter = ".*";
    }
}

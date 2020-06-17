﻿using System;
using System.Collections.Generic;
using System.IO;
using GoAhead.Commands.Selection;

namespace GoAhead.Commands.Wizards
{
    [CommandDescription(Description = "Reads in the system specification with an unplaced island for which a placed net list is provided. Based on this netlist this command inserts a placement into the resultting file", Wrapper = true, Publish = true)]
    public class ProposeIslandPlacement : Command
    {
        protected override void DoCommandAction()
        {
            IslandStyleSystemParameter systemParameter = new IslandStyleSystemParameter();
            systemParameter.Read(this.XMLSpecificationInput);

            if (systemParameter.PartialAreas.Count != 1)
            {
                throw new ArgumentException("Expecting only one island");
            }

            if (!Directory.Exists(this.XMLSpecificationOutputDirectory))
            {
                Directory.CreateDirectory(this.XMLSpecificationOutputDirectory);
            }

            foreach (KeyValuePair<String, PartialAreaSetting> tupel in systemParameter.PartialAreas)
            {
                if (tupel.Value.Modules.Count != 1)
                {
                    throw new ArgumentException("Expecting only one module setting");
                }
                ModuleSetting ms = tupel.Value.Modules[0];
                if (!ms.Settings.ContainsKey("netlist") || !ms.Settings.ContainsKey("name"))
                {
                    throw new ArgumentException("Expecting a value for a netlist and name in the module setting");
                }
                String path = ms.Settings["netlist"];
                String name = ms.Settings["name"];
                // is it a file
                if (!File.Exists(path))
                {
                    throw new ArgumentException("File " + path + " does not exist");
                }

                FindPlacementForReconfigurableArea selCmd = new FindPlacementForReconfigurableArea();
                selCmd.InstancePrefix = name;
                selCmd.XDLModules.Add(path);
                selCmd.TopN = this.TopN;
                selCmd.UserSelectionPrefix = "min_frag_for_placing_an_island_";
                CommandExecuter.Instance.Execute(selCmd);

                for (int i = 1; i < this.TopN; i++)
                {
                    systemParameter.PartialAreas[tupel.Key].Nodes[IslandStyleSystemParameter.Geometry].InnerText = "";

                    foreach (Command cmd in FPGA.TileSelectionManager.Instance.GetListOfAddToSelectionXYCommandsForUserSelection("min_frag_for_placing_an_island_" + i))
                    {
                        systemParameter.PartialAreas[tupel.Key].Nodes[IslandStyleSystemParameter.Geometry].InnerText += cmd.ToString();
                    }

                    String projectDir = this.XMLSpecificationOutputDirectory + @"\run" + i.ToString() + @"\";
                    systemParameter.StaticParameter[IslandStyleSystemParameter.ISEProjectDir].InnerText = projectDir;

                    String specificationOutputFileWithoutExtension = projectDir + System.IO.Path.GetFileNameWithoutExtension(this.XMLSpecificationInput);

                    // save modifications
                    systemParameter.XmlDoc.Save(specificationOutputFileWithoutExtension + ".xml");
                }
            }
        }

        public override void Undo()
        {
        }

        [Parameter(Comment = "The XML system specification to read in")]
        public String XMLSpecificationInput = "system_specification.xml";

        [Parameter(Comment = "The directory where to store the XML system specification with geometry information")]
        public String XMLSpecificationOutputDirectory = "system_specification_proposal.xml";

        [Parameter(Comment = "The first TopN selections will be stored as user selections")]
        public int TopN = 1;
    }
}
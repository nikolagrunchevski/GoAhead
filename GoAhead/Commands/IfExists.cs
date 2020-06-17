﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoAhead.Commands
{
    [CommandDescription(Description = "If the given variable exitsts, execute the commands in the then block, otherwise execute the command in the else block ", Wrapper = false)]
    class IfExists : Command
    {
        // Wrapper=false to keek CommandTrace updates

        protected override void DoCommandAction()
        {
            // internal
            bool exists = Objects.VariableManager.Instance.IsSet(this.Variable);

            // environement
            String value = Environment.GetEnvironmentVariable(this.Variable);
            if (value != null)
            {
                exists = true;
            }

            CommandStringParser parser = new CommandStringParser(exists ? this.Then : this.Else);
            foreach (String cmdString in parser.Parse())
            {
                Command cmd = null;
                String errorDescr = "";
                parser.ParseCommand(cmdString, true, out cmd, out errorDescr);
                // only the alias it self shall appear in the command trace
                cmd.UpdateCommandTrace = false;
                CommandExecuter.Instance.Execute(cmd);
            }
        }

        public override void Undo()
        {
            throw new NotImplementedException();
        }


        [Parameter(Comment = "A variable. Do not embed the variable in %%")]
        public String Variable = "a";

        [Parameter(Comment = "Execute these commands if condition evaluates to true")]
        public String Then = "ClearSelection;InvertSelection;";

        [Parameter(Comment = "Execute these commands if condition evaluates to false")]
        public String Else = "ClearSelection;AddToSelectionXY UpperLeftX=138 UpperLeftY=85 LowerRightX=145 LowerRightY=125;ExpandSelection;";     
    }
}

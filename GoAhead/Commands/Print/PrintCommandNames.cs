﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace GoAhead.Commands
{
    [CommandDescription(Description="Print a list (optionally filtered) of all command names. You may supply this list to an editor for synatx highlighting", Wrapper=false, Publish=true)]
    class PrintCommandNames : Command
    {
        protected override void DoCommandAction()
        {
            try
            {
                Regex.IsMatch(this.Filter, "");
            }
            catch (Exception error)
            {
                throw new ArgumentException("No valid regular expression given: " + this.Filter + " " + error.Message);
            }

            foreach (Type type in CommandStringParser.GetAllCommandTypes().Where(t => String.IsNullOrEmpty(this.Filter) ? true : Regex.IsMatch(t.Name, this.Filter)).OrderBy(t => t.Name))
            {
                this.OutputManager.WriteOutput(type.Name);
            }
        }

        public override void Undo()
        {
            throw new NotImplementedException();
        }

        [Parameter(Comment = "Only print those command the match this regular epxression. Use .* to print all commands or e.g. Print.* to print all commmand that Print something.")]
        public String Filter = ".*";

    }
}

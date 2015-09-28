﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.UI;

using GreySMITH.Revit.Wrappers;

namespace GreySMITH.Commands
{
    public class RibbonPanel_BRplusA : IExternalApplication
    {
        private const string tabName = "BR+A Automation Tools";
        private List<RibbonPanel> panels;

        public Result OnStartup(UIControlledApplication uiContApp)
        {
            // Creates a tab in the Ribbon
            uiContApp.CreateRibbonTab(tabName);

            // Creates a panel in the Tab
            foreach (string panel in CommandList.PanelNames)
            {
                panels.Add(uiContApp.CreateRibbonPanel(tabName, panel));
            }

            AddButtons();

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication uiApplication)
        {
            return Result.Succeeded;
        }

        public void AddButtons()
        {
            // Creates a button for each of the Commands in List
            // Adds the button to the appropiate panel
            foreach (AbstractCommand command in CommandList.Commands)
            {
                var pushbuttondata = new PushButtonData(
                    command.CommandName,
                    description,
                    assemblyLocation,
                    classname);

                try
                {
                    (from panel in panels
                     where panel.Name.Equals(command.PanelName)
                     select panel).FirstOrDefault().AddItem(pushbuttondata);
                }

                catch (NullReferenceException)
                {
                    // eat the error and keep going
                }
            }
        }
    }

    public static class CommandList
    {
        private static Dictionary<AbstractCommand, string> _commands;

        public static void Add(AbstractCommand command)
        {
            _commands.Add(command, command.PanelName);
        }

        /// <summary>
        /// Shoudl only return the unique Panels of the series.
        /// </summary>
        public static IEnumerable<string> PanelNames
        {
            get
            {
                return (from panel in _commands.Values
                    select panel).Distinct();
            }
        }

        public static IEnumerable<string> CommandNames
        {
            get
            {
                return (from command in _commands.Keys
                        select command.CommandName);
            }
        }

        public static IEnumerable<AbstractCommand> Commands
        {
            get { return _commands.Keys; }
        }
    }
}
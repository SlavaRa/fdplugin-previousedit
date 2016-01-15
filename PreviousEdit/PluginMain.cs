using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using ASCompletion.Context;
using PluginCore;
using PluginCore.Helpers;
using PluginCore.Managers;
using PluginCore.Utilities;
using ScintillaNet;

namespace PreviousEdit
{
    public class PluginMain : IPlugin
    {
        readonly Stack<InfoStatus> backward = new Stack<InfoStatus>();
        readonly Stack<InfoStatus> forward = new Stack<InfoStatus>();
        InfoStatus currentStatus;
        InfoStatus executableStatus;
        string settingFilename;
        Settings settingObject;
        ToolStripMenuItem navigateBackwardMenuItem;
        ToolStripMenuItem navigateForwardMenuItem;

        public int Api => 1;
        public string Name => nameof(PreviousEdit);
        public string Guid => "55E1998E-9929-4470-805E-2DB339C29116";
        public string Help => "http://www.flashdevelop.org/community/";
        public string Author => "He Wang";
        public string Description => "Previous Edit Place.";

        [Browsable(false)]
        public object Settings => settingObject;

        public void Initialize()
        {
            InitBasics();
            LoadSettings();
            InitMenuItems();
            AddEventHandlers();
        }

        public void Dispose() => SaveSettings();

        /// <summary>
        /// Initializes important variables
        /// </summary>
        void InitBasics()
        {
            var dataPath = Path.Combine(PathHelper.DataDir, Name);
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            settingFilename = Path.Combine(dataPath, $"{nameof(Settings)}.fdb");
        }

        /// <summary>
        /// Loads the plugin settings
        /// </summary>
        void LoadSettings()
        {
            settingObject = new Settings();
            if (!File.Exists(settingFilename)) SaveSettings();
            else settingObject = (Settings)ObjectSerializer.Deserialize(settingFilename, settingObject);
        }

        void InitMenuItems()
        {
            var menu = (ToolStripMenuItem)PluginBase.MainForm.FindMenuItem("SearchMenu");
            menu.DropDownItems.Add(new ToolStripSeparator());
            var image = PluginBase.MainForm.FindImage("99|9|3|-3");
            navigateBackwardMenuItem = new ToolStripMenuItem("Navigate Backward", image, NavigateBackward);
            PluginBase.MainForm.RegisterShortcutItem($"{Name}.NavigateBackward", navigateBackwardMenuItem);
            menu.DropDownItems.Add(navigateBackwardMenuItem);
            image = PluginBase.MainForm.FindImage("99|9|3|-3");
            navigateForwardMenuItem = new ToolStripMenuItem("Navigate Forward", image, NavigateForward);
            PluginBase.MainForm.RegisterShortcutItem($"{Name}.NavigateForward", navigateForwardMenuItem);
            menu.DropDownItems.Add(navigateForwardMenuItem);
        }

        void UpdateMenuItems()
        {
            navigateBackwardMenuItem.Enabled = backward.Count > 0;
            navigateForwardMenuItem.Enabled = forward.Count > 0;
        }

        void AddEventHandlers() => EventManager.AddEventHandler(this, EventType.FileSwitch);

        public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
        {
            if (e.Type != EventType.FileSwitch) return;
            var doc = PluginBase.MainForm.CurrentDocument;
            if (!doc.IsEditable) return;
            var sci = doc.SciControl;
            sci.UpdateUI += SciControlModified;
            sci.Modified += SciControlModified;
            SciControlModified(sci);
        }

        /// <summary>
        /// Saves the plugin settings
        /// </summary>
        void SaveSettings() => ObjectSerializer.Serialize(settingFilename, settingObject);

        /// <summary>
        /// Modified Handler
        /// </summary>
        void SciControlModified(ScintillaControl sci, int position, int modificationType,
            string text, int length, int linesAdded, int line, int intfoldLevelNow, int foldLevelPrev)
        {
            SciControlModified(sci);
        }

        void SciControlModified(ScintillaControl sci)
        {
            var status = new InfoStatus(sci.FileName, sci.CurrentPos);
            if (currentStatus == null)
            {
                currentStatus = status;
                return;
            }
            if (executableStatus != null)
            {
                executableStatus = null;
                return;
            }
            if (currentStatus.Equals(status)) return;
            backward.Push(currentStatus);
            forward.Clear();
            currentStatus = status;
            UpdateMenuItems();
        }

        void NavigateBackward(object sender, EventArgs e)
        {
            if (backward.Count == 0) return;
            var item = backward.Pop();
            forward.Push(currentStatus);
            Navigate(item);
        }

        void NavigateForward(object sender, EventArgs e)
        {
            if (forward.Count == 0) return;
            var item = forward.Pop();
            backward.Push(currentStatus);
            Navigate(item);
        }

        void Navigate(InfoStatus to)
        {
            UpdateMenuItems();
            executableStatus = to;
            currentStatus = null;
            PluginBase.MainForm.OpenEditableDocument(to.FileName, false);
            var position = to.Position;
            PluginBase.MainForm.CurrentDocument.SciControl.SetSel(position, position);
        }
    }

    class InfoStatus
    {
        public string FileName;
        public int Position;

        public InfoStatus(string fileName, int position)
        {
            FileName = fileName;
            Position = position;
        }

        public override bool Equals(object obj)
        {
            var status = (InfoStatus) obj;
            return status.FileName == FileName && status.Position == Position;
        }

        public override int GetHashCode()
        {
            return FileName.GetHashCode() + Position.GetHashCode();
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using PluginCore;
using PluginCore.Helpers;
using PluginCore.Managers;
using PluginCore.Utilities;
using ProjectManager;
using ScintillaNet;

namespace PreviousEdit
{
    public class PluginMain : IPlugin
    {
        readonly List<InfoStatus> backward = new List<InfoStatus>();
        readonly List<InfoStatus> forward = new List<InfoStatus>();
        InfoStatus currentStatus;
        InfoStatus executableStatus;
        string settingFilename;
        Settings settingObject;
        List<ToolStripItem> forwardMenuItems;
        List<ToolStripItem> backwardMenuItems;

        public int Api => 1;
        public string Name => nameof(PreviousEdit);
        public string Guid => "55E1998E-9929-4470-805E-2DB339C29116";
        public string Help => "http://www.flashdevelop.org/community/";
        public string Author => "He Wang, SlavaRa";
        public string Description => "Navigate Backward and Navigate Forward";

        [Browsable(false)]
        public object Settings => settingObject;

        public void Initialize()
        {
            InitBasics();
            LoadSettings();
            backward.Capacity = ((Settings)Settings).MaxBackward;
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
            var menu = (ToolStripMenuItem) PluginBase.MainForm.FindMenuItem("SearchMenu");
            menu.DropDownItems.Add(new ToolStripSeparator());
            backwardMenuItems = createMenuItem(menu, "1", "Navigate Backward", NavigateBackward, $"{Name}.NavigateBackward", 0);
            forwardMenuItems = createMenuItem(menu, "9", "Navigate Forward", NavigateForward, $"{Name}.NavigateForward", 1);
            PluginBase.MainForm.ToolStrip.Items.Insert(2, new ToolStripSeparator());
        }

        static List<ToolStripItem> createMenuItem(ToolStripDropDownItem menu, string imageIndex, string text, EventHandler onClick, string shortcutId, int toolbarIndex)
        {
            var image = PluginBase.MainForm.FindImage(imageIndex);
            var menuItem = new ToolStripMenuItem(text, image, onClick);
            PluginBase.MainForm.RegisterShortcutItem(shortcutId, menuItem);
            menu.DropDownItems.Add(menuItem);
            var toolbarItem = new ToolStripButton(string.Empty, image, onClick) {ToolTipText = text};
            PluginBase.MainForm.ToolStrip.Items.Insert(toolbarIndex, toolbarItem);
            return new List<ToolStripItem> {menuItem, toolbarItem};
        }

        void UpdateMenuItems()
        {
            backwardMenuItems.ForEach(it => it.Enabled = backward.Count > 0);
            forwardMenuItems.ForEach(it => it.Enabled = forward.Count > 0);
        }

        void AddEventHandlers() => EventManager.AddEventHandler(this, EventType.FileSwitch | EventType.Command);

        public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
        {
            if (e.Type == EventType.Command && ((DataEvent) e).Action == ProjectManagerEvents.Project)
            {
                backward.Clear();
                forward.Clear();
                UpdateMenuItems();
                return;
            }
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
            if (backward.Count == backward.Capacity - 1) backward.RemoveAt(0);
            backward.Add(currentStatus);
            forward.Clear();
            currentStatus = status;
            UpdateMenuItems();
        }

        void NavigateBackward(object sender, EventArgs e)
        {
            var count = backward.Count;
            if (count == 0) return;
            var item = backward[count - 1];
            backward.Remove(item);
            forward.Add(currentStatus);
            Navigate(item);
        }

        void NavigateForward(object sender, EventArgs e)
        {
            var count = forward.Count;
            if (count == 0) return;
            var item = forward[count - 1];
            forward.Remove(item);
            backward.Add(currentStatus);
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using PluginCore;
using PluginCore.Helpers;
using PluginCore.Managers;
using PluginCore.Utilities;
using PreviousEdit.Behavior;
using ProjectManager;
using ScintillaNet;
using ScintillaNet.Enums;

namespace PreviousEdit
{
    public class PluginMain : IPlugin
    {
        readonly VSBehavior behavior = new VSBehavior();
        QueueItem executableStatus;
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
            backwardMenuItems.ForEach(it => it.Enabled = behavior.CanBackward);
            forwardMenuItems.ForEach(it => it.Enabled = behavior.CanForward);
        }

        void AddEventHandlers() => EventManager.AddEventHandler(this, EventType.FileSwitch | EventType.Command);

        public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
        {
            if (e.Type == EventType.Command && ((DataEvent) e).Action == ProjectManagerEvents.Project)
            {
                behavior.Clear();
                executableStatus = null;
                UpdateMenuItems();
                return;
            }
            if (e.Type != EventType.FileSwitch) return;
            var doc = PluginBase.MainForm.CurrentDocument;
            if (!doc.IsEditable) return;
            var sci = doc.SciControl;
            sci.Modified -= SciControlModified;
            sci.Modified += SciControlModified;
            sci.UpdateUI -= SciControlUpdateUI;
            sci.UpdateUI += SciControlUpdateUI;
            SciControlUpdateUI(sci);
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
            if ((modificationType & (int)ModificationFlags.DeleteText) > 0)
            {
                TraceManager.Add($"{nameof(SciControlModified)}:{nameof(modificationType)}:{modificationType}");
            }
            //if (linesAdded > 0)
            //{
            //    foreach (var it in backward)
            //    {
            //        var startLine = sci.CurrentLine - linesAdded;
            //        if (it.Line < startLine) continue;
            //        it.Line += linesAdded;
            //        for (int i = startLine; i < linesAdded; i++)
            //        {
            //            it.Position += sci.LineLength(i);
            //        }
            //    }
            //    backward.ForEach(it => TraceManager.Add($"{it.Line}:{it.Position}"));
            //    currentStatus = GetStatus(sci);
            //}
            //TODO slavara: remove lines
        }

        void SciControlUpdateUI(ScintillaControl sci)
        {
            if (executableStatus != null && executableStatus.Equals(behavior.CurrentItem)) return;
            behavior.Add(sci.FileName, sci.CurrentPos, sci.CurrentLine);
            UpdateMenuItems();
        }

        void NavigateBackward(object sender, EventArgs e)
        {
            if (!behavior.CanBackward) return;
            behavior.Backward();
            Navigate(behavior.CurrentItem);
        }

        void NavigateForward(object sender, EventArgs e)
        {
            if (!behavior.CanForward) return;
            behavior.Forward();
            Navigate(behavior.CurrentItem);
        }

        void Navigate(QueueItem to)
        {
            executableStatus = to;
            UpdateMenuItems();
            PluginBase.MainForm.OpenEditableDocument(to.FileName, false);
            var position = to.Position;
            PluginBase.MainForm.CurrentDocument.SciControl.SetSel(position, position);
            executableStatus = null;
        }
    }
}
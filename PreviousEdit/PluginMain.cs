using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
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
        public int Api => 1;

        [NotNull]
        public string Name => nameof(PreviousEdit);
        public string Guid => "55E1998E-9929-4470-805E-2DB339C29116";
        public string Help => "http://www.flashdevelop.org/community/";
        public string Author => "He Wang, SlavaRa";
        public string Description => "Navigate Backward and Navigate Forward";

        [Browsable(false)]
        public object Settings => settingObject;
        string settingFilename;
        Settings settingObject;
        readonly VSBehavior behavior = new VSBehavior();
        QueueItem executableStatus;
        List<ToolStripItem> forwardMenuItems;
        List<ToolStripItem> backwardMenuItems;
        int sciPrevPosition;

        /// <summary>
        /// Initializes the plugin
        /// </summary>
        public void Initialize()
        {
            InitBasics();
            LoadSettings();
            CreateMenuItems();
            AddEventHandlers();
        }

        /// <summary>
        /// Disposes the plugin
        /// </summary>
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

        /// <summary>
        /// Creates the required menu items
        /// </summary>
        void CreateMenuItems()
        {
            var menu = (ToolStripMenuItem) PluginBase.MainForm.FindMenuItem("SearchMenu");
            menu.DropDownItems.Add(new ToolStripSeparator());
            backwardMenuItems = CreateMenuItem(menu, "1", "Navigate Backward", OnBackwardClick, $"{Name}.NavigateBackward", 0);
            forwardMenuItems = CreateMenuItem(menu, "9", "Navigate Forward", NavigateForward, $"{Name}.NavigateForward", 1);
            PluginBase.MainForm.ToolStrip.Items.Insert(2, new ToolStripSeparator());
        }

        static List<ToolStripItem> CreateMenuItem(ToolStripDropDownItem menu, string imageIndex, string text, EventHandler onClick, string shortcutId, int toolbarIndex)
        {
            var image = PluginBase.MainForm.FindImage(imageIndex);
            var menuItem = new ToolStripMenuItem(text, image, onClick);
            PluginBase.MainForm.RegisterShortcutItem(shortcutId, menuItem);
            menu.DropDownItems.Add(menuItem);
            ToolStripItem toolbarItem;
            if (toolbarIndex == 0)
            {
                toolbarItem = new ToolStripSplitButton(string.Empty, image, onClick) {ToolTipText = text};
                var imageList = new ImageList();
                imageList.Images.Add("empty", PluginBase.MainForm.FindImage("-1"));
                imageList.Images.Add("current", PluginBase.MainForm.FindImage("461"));
                var button = (ToolStripSplitButton) toolbarItem;
                button.DropDown.ImageList = imageList;
                button.DropDown.MaximumSize = new Size(340, 600);
            }
            else toolbarItem = new ToolStripButton(string.Empty, image, onClick) {ToolTipText = text};
            PluginBase.MainForm.ToolStrip.Items.Insert(toolbarIndex, toolbarItem);
            return new List<ToolStripItem> {menuItem, toolbarItem};
        }

        /// <summary>
        /// Updates the state of the menu items
        /// </summary>
        void UpdateMenuItems()
        {
            backwardMenuItems.ForEach(it => it.Enabled = behavior.CanBackward);
            forwardMenuItems.ForEach(it => it.Enabled = behavior.CanForward);
            var button = (ToolStripSplitButton)backwardMenuItems[1];
            button.DropDownItems.Clear();
            var backward = behavior.GetBackward();
            for (var i = backward.Count - 1; i >= 0; i--)
            {
                var it = backward[i];
                button.DropDownItems.Add($"{Path.GetFileName(it.FileName)}: Line {it.Line}: Position {it.Position}");
            }
            var item = behavior.CurrentItem;
            button.DropDownItems.Add($"{Path.GetFileName(item.FileName)}: Line {item.Line}: Position {item.Position}");
            var forward = behavior.GetForward();
            foreach (var it in forward)
            {
                button.DropDownItems.Add($"{Path.GetFileName(it.FileName)}: Line {it.Line}: Position {it.Position}");
            }
#if DEBUG
//            button.DropDown.AutoClose = false;
//            button.DropDown.Show();
#endif
        }

        void AddEventHandlers()
        {
            EventManager.AddEventHandler(this, EventType.FileSwitch | EventType.Command);
            behavior.Change += BehaviorOnChange;
        }

        /// <summary>
        /// Handles the incoming events
        /// </summary>
        public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
        {
            if (e.Type == EventType.Command && ((DataEvent) e).Action == ProjectManagerEvents.Project)
            {
                behavior.Clear();
                sciPrevPosition = 0;
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
            if (modificationType != (int) ModificationFlags.DeleteText &&
                modificationType != (int) ModificationFlags.InsertText) return;
            var startPosition = sciPrevPosition < position ? sciPrevPosition : position;
            if (linesAdded < 0) length = -length;
            behavior.Update(sci.FileName, startPosition, length, linesAdded);
            sciPrevPosition = sci.CurrentPos;
        }
        
        void SciControlUpdateUI([NotNull] ScintillaControl sci)
        {
            var currentPos = sci.CurrentPos;
            if (currentPos == sciPrevPosition) return;
            if (executableStatus != null && executableStatus.Equals(behavior.CurrentItem)) return;
            if (string.IsNullOrEmpty(sci.FileName)) return;
            behavior.Add(sci.FileName, currentPos, sci.CurrentLine);
            UpdateMenuItems();
            sciPrevPosition = currentPos;
        }

        void OnBackwardClick(object sender, EventArgs e)
        {
            if (((ToolStripSplitButton) sender).ButtonPressed && behavior.CanBackward) behavior.Backward();
        }
        
        void NavigateForward(object sender, EventArgs e)
        {
            if (behavior.CanForward) behavior.Forward();
        }

        void BehaviorOnChange(object sender, EventArgs e) => Navigate(behavior.CurrentItem);

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
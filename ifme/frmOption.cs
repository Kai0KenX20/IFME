﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using IniParser;
using IniParser.Model;

namespace ifme
{
	public partial class frmOption : Form
	{
		StringComparison IC = StringComparison.OrdinalIgnoreCase;

		public frmOption()
		{
			InitializeComponent();
			Icon = Properties.Resources.control_equalizer_blue;

			btnBrowse.Image = Properties.Resources.folder_explore;
		}

		private void frmOption_Load(object sender, EventArgs e)
		{
			// General
			txtTempFolder.Text = Properties.Settings.Default.DirTemp;
			txtNamePrefix.Text = Properties.Settings.Default.NamePrefix;
			chkSoundDone.Checked = Properties.Settings.Default.SoundFinish;

			// Load CPU stuff
			for (int i = 0; i < Environment.ProcessorCount; i++)
			{
				if (i >= 2)
					clbCPU.Items.Add("CPU " + (i + 1).ToString(), TaskManager.CPU.Affinity[i]);
				else
					clbCPU.Items.Add("CPU " + (i + 1).ToString(), TaskManager.CPU.Affinity[i]);
			}

			cboCPUPriority.SelectedIndex = Properties.Settings.Default.Nice;

			// AviSynth
			lblAviSynthStatus.Text = Plugin.AviSynthInstalled ? Language.Installed : Language.NotInstalled;
			lblAviSynthStatus.ForeColor = Plugin.AviSynthInstalled ? Color.Green : Color.Red;

			if (Plugin.AviSynthInstalled)
			{
				if (string.Equals(CRC32.GetFile(Plugin.AviSynthFile), "0x073A3318"))
				{
					lblAviSynthStatus.Text += ", 2.6 MT (2015.02.20)";
				}
				else if (string.Equals(CRC32.GetFile(Plugin.AviSynthFile), "0x30E0D263"))
				{
					lblAviSynthStatus.Text += ", 2.6 ST (Original)";
				}
				else
				{
					lblAviSynthStatus.Text += " (Unknown)";
				}
			}

			txtAvsDecoder.Text = Properties.Settings.Default.AvsDecoder;
			chkCopyContentMKV.Checked = Properties.Settings.Default.AvsMkvCopy;

			// Plugin
			foreach (var item in Plugin.List)
			{
				ListViewItem x = new ListViewItem(new[] { 
					item.Profile.Name,
					item.Profile.Ver,
					item.Profile.Dev,
					item.Provider.Name
				});

				x.Tag = item.Profile.Web;

				lstPlugin.Items.Add(x);
			}

			// Extension
			foreach (var item in Extension.Items)
			{
				ListViewItem x = new ListViewItem(new[] {
					$"{item.Name} ({item.FileName})",
					item.Type,
					item.Version,
					item.Developer
				});

				x.Tag = item.UrlWeb;

				lstExtension.Items.Add(x);

				// List all default extension
				if (string.Equals(item.Type, "notepad", IC))
					cboDefaultEditor.Items.Add($"{item.Name} ({item.FileName})");
				else if (string.Equals(item.Type, "benchmark", IC))
					cboDefaultBenchmark.Items.Add($"{item.Name} ({item.FileName})");
			}

			for (int i = 0; i < cboDefaultEditor.Items.Count; i++)
			{
				if (((string)cboDefaultEditor.Items[i]).Contains(Properties.Settings.Default.DefaultNotepad))
				{
					cboDefaultEditor.SelectedIndex = i;
					break; // stop found default notepad
				}
			}

			for (int i = 0; i < cboDefaultBenchmark.Items.Count; i++)
			{
				if (((string)cboDefaultBenchmark.Items[i]).Contains(Properties.Settings.Default.DefaultBenchmark))
				{
					cboDefaultBenchmark.SelectedIndex = i;
					break; // stop found default notepad
				}
			}

			// Profile
			foreach (var item in Profile.List)
			{
				ListViewItem x = new ListViewItem(new[] {
					item.Info.Name,
					item.Info.Format,
					item.Info.Platform,
					item.Info.Author
				});

				x.Tag = item.Info.Web;

				lstProfile.Items.Add(x);
			}

			// Compiler
			if (string.Equals(Properties.Settings.Default.Compiler, "icc", IC))
				rdoCompilerIntel.Checked = true;
			else if (string.Equals(Properties.Settings.Default.Compiler, "msvc", IC))
				rdoCompilerMicrosoft.Checked = true;
			else
				rdoCompilerGCC.Checked = true;

			if (!Plugin.IsExistHEVCICC)
				rdoCompilerIntel.Enabled = false;

			if (!Plugin.IsExistHEVCMSVC)
				rdoCompilerMicrosoft.Enabled = false;

			// Language
			foreach (var item in Language.Lists)
			{
				cboLang.Items.Add(item.Name);

				if (string.Equals(Properties.Settings.Default.Language, item.Code))
					cboLang.Text = item.Name;
			}

			// Language
#if MAKELANG
			LangCreate();
#else
			LangApply();
#endif
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog GetFolder = new FolderBrowserDialog();

			GetFolder.Description = "";
			GetFolder.ShowNewFolderButton = true;
			GetFolder.RootFolder = Environment.SpecialFolder.MyComputer;

			if (!string.IsNullOrEmpty(txtTempFolder.Text))
			{
				GetFolder.SelectedPath = txtTempFolder.Text;
			}

			if (GetFolder.ShowDialog() == DialogResult.OK)
			{
				if (Directory.EnumerateFileSystemEntries(GetFolder.SelectedPath).Any())
				{
					MessageBox.Show("Please choose an empty folder", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				else
				{
					txtTempFolder.Text = GetFolder.SelectedPath;
					Properties.Settings.Default.DirTemp = GetFolder.SelectedPath;
				}
			}
		}

		private void txtNamePrefix_TextChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.NamePrefix = txtNamePrefix.Text;
		}

		private void chkSoundDone_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.SoundFinish = chkSoundDone.Checked;
		}

		private void cboDefaultEditor_SelectedIndexChanged(object sender, EventArgs e)
		{
			string item = cboDefaultEditor.Text;
			Properties.Settings.Default.DefaultNotepad = item.Substring(item.IndexOf('(') + 1).Replace(")", "");
		}

		private void cboDefaultBenchmark_SelectedIndexChanged(object sender, EventArgs e)
		{
			string item = cboDefaultBenchmark.Text;
			Properties.Settings.Default.DefaultBenchmark = item.Substring(item.IndexOf('(') + 1).Replace(")", "");
		}

		private void txtAvsDecoder_TextChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.AvsDecoder = txtAvsDecoder.Text;
		}

		private void chkCopyContentMKV_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.AvsMkvCopy = chkCopyContentMKV.Checked;
		}

		private void lblHFR_Click(object sender, EventArgs e)
		{
			Process.Start("http://www.spirton.com/interframe/");
		}

		private void rdoCompilerGCC_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.Compiler = "gcc";
		}

		private void rdoCompilerIntel_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.Compiler = "icc";
		}

		private void rdoCompilerMicrosoft_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.Compiler = "msvc";
		}

		private void tsmiPluginWeb_Click(object sender, EventArgs e)
		{
			if (lstPlugin.SelectedItems.Count == 1)
				Process.Start((string)lstPlugin.SelectedItems[0].Tag);
		}

		private void tsmiExtensionWeb_Click(object sender, EventArgs e)
		{
			if (lstExtension.SelectedItems.Count == 1)
				Process.Start((string)lstExtension.SelectedItems[0].Tag);
		}

		private void tsmiProfileWeb_Click(object sender, EventArgs e)
		{
			if (lstProfile.SelectedItems.Count == 1)
				Process.Start((string)lstProfile.SelectedItems[0].Tag);
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			if (chkReset.Checked)
			{
				Properties.Settings.Default.Reset();
				Properties.Settings.Default.Save();
				return;
			}

			// Save CPU affinity
			string aff = "";
			for (int i = 0; i < Environment.ProcessorCount; i++)
			{
				TaskManager.CPU.Affinity[i] = clbCPU.GetItemChecked(i);
				aff += clbCPU.GetItemChecked(i).ToString() + ",";
			}
			aff = aff.Remove(aff.Length - 1);
			Properties.Settings.Default.CPUAffinity = aff;
			Properties.Settings.Default.Nice = cboCPUPriority.SelectedIndex;

			// Language
			if (cboLang.SelectedIndex >= 0)
				Properties.Settings.Default.Language = Language.Lists[cboLang.SelectedIndex].Code;

			// Save
			Properties.Settings.Default.Save();

			// Compiler
			Plugin.HEVCL = Path.Combine(Global.Folder.Plugins, $"x265{Properties.Settings.Default.Compiler}", "x265lo");
			Plugin.HEVCH = Path.Combine(Global.Folder.Plugins, $"x265{Properties.Settings.Default.Compiler}", "x265hi");
		}

		void LangApply()
		{
			var data = Language.Get;

			Control ctrl = this;
			do
			{
				ctrl = GetNextControl(ctrl, true);

				if (ctrl != null)
					if (ctrl is Label ||
						ctrl is Button ||
						ctrl is TabPage ||
						ctrl is CheckBox ||
						ctrl is RadioButton ||
						ctrl is GroupBox)
						if (!string.IsNullOrEmpty(ctrl.Text))
							ctrl.Text = data[Name][ctrl.Name].Replace("\\n", "\n");

			} while (ctrl != null);

			foreach (ColumnHeader item in lstPlugin.Columns)
				item.Text = data[Name][$"{item.Tag}"];

			foreach (ColumnHeader item in lstExtension.Columns)
				item.Text = data[Name][$"{item.Tag}"];

			foreach (ColumnHeader item in lstProfile.Columns)
				item.Text = data[Name][$"{item.Tag}"];
		}

		void LangCreate()
		{
			var parser = new FileIniDataParser();
			IniData data = parser.ReadFile(Path.Combine(Global.Folder.Language, "en.ini"));

			data.Sections.AddSection(Name);
			Control ctrl = this;
			do
			{
				ctrl = GetNextControl(ctrl, true);

				if (ctrl != null)
					if (ctrl is Label ||
						ctrl is Button ||
						ctrl is TabPage ||
						ctrl is CheckBox ||
						ctrl is RadioButton ||
						ctrl is GroupBox)
						if (!string.IsNullOrEmpty(ctrl.Text))
							data.Sections[Name].AddKey(ctrl.Name, ctrl.Text.Replace("\n", "\\n").Replace("\r", ""));

			} while (ctrl != null);

			foreach (ColumnHeader item in lstPlugin.Columns)
				data.Sections[Name].AddKey($"colPlugin{item.Text}", item.Text);

			foreach (ColumnHeader item in lstExtension.Columns)
				data.Sections[Name].AddKey($"colExtension{item.Text}", item.Text);

			foreach (ColumnHeader item in lstProfile.Columns)
				data.Sections[Name].AddKey($"colProfile{item.Text}", item.Text);

			data.Sections[Name].AddKey("Installed", Language.Installed);
			data.Sections[Name].AddKey("NotInstalled", Language.NotInstalled);

			parser.WriteFile(Path.Combine(Global.Folder.Language, "en.ini"), data);
		}
	}
}
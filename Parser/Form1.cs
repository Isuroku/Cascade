﻿using CascadeParser;
using CascadeUnitTest;
using CascadeSerializer;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Parser
{
    public partial class Form1 : Form, ILogPrinter, IParserOwner
    {
        const string _path_to_data = "Data";

		string _current_path;

		string _selected_file;

        CParserManager _parser;

        IKey _test_serialize;

        public Form1()
        {
            InitializeComponent();
        }

        #region LogWindow Output
        void AddLogToRichText(string inText, Color inClr)
        {
            if (m_uiLogLinesCount > 1000)
                ClearLog();

            int length = rtLog.TextLength;  // at end of text
            rtLog.AppendText(inText);
            rtLog.SelectionStart = length;
            rtLog.SelectionLength = inText.Length;
            rtLog.SelectionColor = inClr;
            rtLog.SelectionStart = rtLog.TextLength;
            rtLog.SelectionLength = 0;
        }

        void ClearLog()
        {
            rtLog.Text = string.Empty;
            m_uiLogLinesCount = 0;
        }

        uint m_uiLogLinesCount = 0;
        public void AddLogToConsole(string inText, Color inClr)
        {
            if (rtLog.IsDisposed)
                return;

            string sres = string.Format("{0}: {1}{2}", m_uiLogLinesCount.ToString(), inText, Environment.NewLine);
            rtLog.BeginInvoke(new Action<string>(s => AddLogToRichText(s, inClr)), sres);
            m_uiLogLinesCount++;
        }

        public enum ELogLevel { Info, Warning, Error }

        public void AddLogToConsole(string inText, ELogLevel inLogLevel)
        {
            if (rtLog.IsDisposed)
                return;

            string sres = string.Format("{0}: {1}{2}", m_uiLogLinesCount.ToString(), inText, Environment.NewLine);

            Color clr = Color.Black;
            switch (inLogLevel)
            {
                case ELogLevel.Info: clr = Color.Black; break;
                case ELogLevel.Warning: clr = Color.Brown; break;
                case ELogLevel.Error: clr = Color.Red; break;
            }

            //tbLog.BeginInvoke(new Action<string>(s => tbLog.AppendText(s)), sres);
            rtLog.BeginInvoke(new Action<string>(s => AddLogToRichText(s, clr)), sres);
            m_uiLogLinesCount++;
        }

        //ILogger
        public void LogWarning(string inText)
        {
            AddLogToConsole(inText, ELogLevel.Warning);
        }

        public void LogError(string inText)
        {
            AddLogToConsole(inText, ELogLevel.Error);
        }

        public void Trace(string inText)
        {
            AddLogToConsole(inText, ELogLevel.Info);
        }

        #endregion //LogWindow Output

        private void Form1_Load(object sender, EventArgs e)
        {
            _parser = new CParserManager(this);

			string path = Path.Combine(Application.StartupPath, "prev_path.txt");
			if (File.Exists(path))
			{
				_current_path = File.ReadAllText(path);
			}
			else
			{
				_current_path = Application.StartupPath;
				path = Path.Combine(Application.StartupPath, _path_to_data);
				if (Directory.Exists(path))
					_current_path = path;
			}

			LoadFiles();
        }

		private void LoadFiles()
		{
			if (!Directory.Exists(_current_path))
				return;

			string[] files = Directory.GetFiles(_current_path, "*.csc??", SearchOption.TopDirectoryOnly);

			lbSourceFiles.Items.Clear();
			foreach (var fn in files)
			{
				string only_name = Path.GetFileName(fn);
				lbSourceFiles.Items.Add(only_name);
			}

			if (lbSourceFiles.Items.Count > 0)
				lbSourceFiles.SelectedIndex = 0;
		}

		public string GetTextFromFile(string inFileName, object inContextData)
        {
            string path = Path.Combine(_current_path, inFileName);
            if (!File.Exists(path))
                return string.Empty;
            return File.ReadAllText(path);
        }

        private void lbSourceFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            string fn = lbSourceFiles.SelectedItem.ToString();
            _selected_file = Path.Combine(_current_path, fn);

            tbSourceText.Text = File.ReadAllText(_selected_file);
        }

        private void tbSourceText_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbSourceText.Text) ||
                string.IsNullOrEmpty(_selected_file))
                return;

            File.WriteAllText(_selected_file, tbSourceText.Text);
        }

        private void btnParse_Click(object sender, EventArgs e)
        {
            ClearLog();

			//FileBrowser.Url;

			if (string.IsNullOrEmpty(tbSourceText.Text))
				return;

			_test_serialize = _parser.Parse(Path.GetFileName(_selected_file), tbSourceText.Text, this, 98);
            ShowTokenLines(_test_serialize);
            AddToTree(_test_serialize);
        }

        void ShowTokenLines(IKey key)
        {
            CTokenLine[] lines = _parser.GetLineByRoot(key);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                sb.Append(string.Format("{0}: {1}{2}", i.ToString("D4"), lines[i], Environment.NewLine));
            }

            tbResult.Text = sb.ToString();
        }

        void AddToTree(IKey key)
        {
            tvTree.Nodes.Clear();
            AddToTree(key, tvTree.Nodes);
            tvTree.ExpandAll();
        }

        void AddToTree(IKey key, TreeNodeCollection nc)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < key.GetValuesCount(); i++)
            {
                IKeyValue value = key.GetValue(i);

                string val_comments = string.Empty;
                if (!string.IsNullOrEmpty(value.Comments))
                    val_comments = string.Format("[//{0}]", value.Comments);

                sb.AppendFormat("{0}{1}, ", value, val_comments);
            }

            string arr_flag = string.Empty;
            if(key.IsArrayKey())
                arr_flag = "[a]";

            string key_comments = string.Empty;
            if (!string.IsNullOrEmpty(key.Comments))
                key_comments = string.Format(" //{0}", key.Comments);

            TreeNode tn = new TreeNode(string.Format("{0}{1}: {2}{3}", key.GetName(), arr_flag, sb, key_comments));
            nc.Add(tn);

            for(int i = 0; i < key.GetChildCount(); i++)
            {
                IKey el = key.GetChild(i);
                AddToTree(el, tn.Nodes);
            }
        }

        private void btnNewFile_Click(object sender, EventArgs e)
        {
            SaveTextToFile("", tbNewFileName.Text, true);
        }

        void SaveTextToFile(string inText, string inFileName, bool inCheckExists)
        {
            string new_file_name = inFileName;
            if(string.IsNullOrEmpty(Path.GetExtension(new_file_name)))
                new_file_name = inFileName + ".cscd";

            string path = Path.Combine(_current_path, new_file_name);

            if (inCheckExists && File.Exists(path))
            {
                AddLogToConsole(string.Format("File {0} already exists", new_file_name), ELogLevel.Error);
                return;
            }

            try
            {
                File.WriteAllText(path, inText, Encoding.UTF8);
            }
            catch(Exception ex)
            {
                AddLogToConsole($"Can't write to {new_file_name}. Exception msg: {ex.Message}", ELogLevel.Error);
                return;
            }

            int index = lbSourceFiles.Items.IndexOf(new_file_name);
            if (index == -1)
            {
                lbSourceFiles.Items.Add(new_file_name);
                lbSourceFiles.SelectedIndex = lbSourceFiles.Items.Count - 1;
            }
            else
                lbSourceFiles.SelectedIndex = index;
        }

        private void btnSerializeTests_Click(object sender, EventArgs e)
        {
            var serializer = new CCascadeSerializer();
            //TestObject saved_obj = TestObject.CreateTestObject();
            CCharacterDescr saved_obj = CCharacterDescr.CreateTestObject();

            _test_serialize = serializer.SerializeToKey(saved_obj, string.Empty, this);

            string text = _test_serialize.SaveToString();
            SaveTextToFile(text, "SerializeTest", false);

            tvTree.Nodes.Clear();
            AddToTree(_test_serialize, tvTree.Nodes);
            tvTree.ExpandAll();
        }

        private void btnDeserializeTest_Click(object sender, EventArgs e)
        {
            ClearLog();

            _test_serialize = _parser.Parse(Path.GetFileName(_selected_file), tbSourceText.Text, this, null);
            ShowTokenLines(_test_serialize);
            AddToTree(_test_serialize);

            var serializer = new CCascadeSerializer(_parser);
            var saved_obj = serializer.Deserialize<CShipMalfunctionsDescr[]>(_test_serialize, this);
            //CTestBase saved_obj = serializer.Deserialize<CTestBase>(Path.GetFileName(_selected_file), tbSourceText.Text, this, null);
        }

        private void btnSaveToFile_Click(object sender, EventArgs e)
        {
            if(_test_serialize == null)
            {
                AddLogToConsole("Test Serialize doesnt present.", ELogLevel.Error);
                return;
            }
            string text = _test_serialize.SaveToString();
            SaveTextToFile(text, "SerializeTest", false);
        }

		private void btnOpenNewFolder_Click(object sender, EventArgs e)
		{
			using(var fbd = new FolderBrowserDialog() { Description = "Select path" })
			{
				fbd.SelectedPath = _current_path;
				if (fbd.ShowDialog() == DialogResult.OK)
				{
					_current_path = fbd.SelectedPath;
					string path = Path.Combine(Application.StartupPath, "prev_path.txt");
					File.WriteAllText(path, _current_path, Encoding.UTF8);
					LoadFiles();
				}
			}
		}
	}
}

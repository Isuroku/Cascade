using HLDParser;
using ReflectionSerializer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Parser
{
    public partial class Form1 : Form, ILogPrinter, IParserOwner, ILogger
    {
        const string _path_to_data = "Data";

        string _selected_file;

        CParserManager _parser;

        CKey _test_serialize;

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
                case ELogLevel.InternalError: clr = Color.Green; break;
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
            _parser = new CParserManager(this, this);

            string path = Path.Combine(Application.StartupPath, _path_to_data);
            string[] files = Directory.GetFiles(path, "*.txt", SearchOption.TopDirectoryOnly);

            foreach (var fn in files)
            {
                string only_name = Path.GetFileName(fn);
                lbSourceFiles.Items.Add(only_name);
            }

            if (lbSourceFiles.Items.Count > 0)
                lbSourceFiles.SelectedIndex = 0;
        }

        public string GetTextFromFile(string inFileName)
        {
            string path = Path.Combine(Application.StartupPath, _path_to_data, inFileName);
            if (!File.Exists(path))
                return string.Empty;
            return File.ReadAllText(path);
        }

        private void lbSourceFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            string fn = lbSourceFiles.SelectedItem.ToString();
            _selected_file = Path.Combine(Application.StartupPath, _path_to_data, fn);

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

            tvTree.Nodes.Clear();

            _test_serialize = _parser.Parse(Path.GetFileName(_selected_file), tbSourceText.Text);

            CTokenLine[] lines = _parser.GetLineByRoot(_test_serialize);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                sb.Append(string.Format("{0}: {1}{2}", i.ToString("D4"), lines[i], Environment.NewLine));
            }
            
            tbResult.Text = sb.ToString();

            AddToTree(_test_serialize, tvTree.Nodes);
            tvTree.ExpandAll();
        }

        void AddToTree(CBaseKey key, TreeNodeCollection nc)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < key.ValuesCount; i++)
            {
                CBaseElement el = key.GetValue(i);
                sb.AppendFormat("{0}, ", el);
            }

            string arr_flag = string.Empty;
            if(key.GetElementType() == EElementType.ArrayKey)
                arr_flag = "[a]";

            string key_comments = string.Empty;
            if (!string.IsNullOrEmpty(key.Comments))
                key_comments = string.Format(" //{0}", key.Comments);

            TreeNode tn = new TreeNode(string.Format("{0}{1}: {2}{3}", key.Name, arr_flag, sb, key_comments));
            nc.Add(tn);

            for(int i = 0; i < key.KeyCount; i++)
            {
                CBaseKey el = key.GetKey(i);
                AddToTree(el as CBaseKey, tn.Nodes);
            }
        }

        private void btnNewFile_Click(object sender, EventArgs e)
        {
            SaveTextToFile("", tbNewFileName.Text, true);
        }

        void SaveTextToFile(string inText, string inFileName, bool inCheckExists)
        {
            string new_file_name = inFileName + ".txt";
            string path = Path.Combine(Application.StartupPath, _path_to_data, new_file_name);

            if (inCheckExists && File.Exists(path))
            {
                AddLogToConsole(string.Format("File {0} already exists", new_file_name), ELogLevel.Error);
                return;
            }

            File.WriteAllText(path, inText, Encoding.UTF8);

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
            var serializer = new CKeySerializer(new CachedReflector());
            //TestObject saved_obj = TestObject.CreateTestObject();
            CTest2 saved_obj = CTest2.CreateTestObject();
            _test_serialize = new CKey(null, "TestObject");

            serializer.Serialize(saved_obj, _test_serialize, this);

            //var serializer = new Serializer(new CachedReflector());
            //SerializedObject serialized = serializer.Serialize(TestObject.CreateTestObject());
            ////SerializedObject serialized = serializer.Serialize(new CTest2());
            //string text = serialized.Stringify();
            //AddLogToRichText(text, Color.Green);

            //CKey root = new CKey(null, serialized.Name);
            //SaveToRoot(root, serialized);

            tvTree.Nodes.Clear();
            AddToTree(_test_serialize, tvTree.Nodes);
            tvTree.ExpandAll();
        }

        private void btnDeserializeTest_Click(object sender, EventArgs e)
        {
            var serializer = new CKeySerializer(new CachedReflector());
            CTest2 saved_obj = serializer.Deserialize<CTest2>(_test_serialize, this);
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
    }
}

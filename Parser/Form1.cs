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

            CKey root = _parser.Parse(Path.GetFileName(_selected_file), tbSourceText.Text);

            CTokenLine[] lines = _parser.GetLineByRoot(root);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                sb.Append(string.Format("{0}: {1}{2}", i.ToString("D4"), lines[i], Environment.NewLine));
            }
            
            tbResult.Text = sb.ToString();

            AddToTree(root, tvTree.Nodes);
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

            TreeNode tn = new TreeNode(string.Format("{0}: {1}", key.Name, sb));
            nc.Add(tn);
            for(int i = 0; i < key.KeyCount; i++)
            {
                CBaseKey el = key.GetKey(i);
                AddToTree(el as CBaseKey, tn.Nodes);
            }
        }

        private void btnNewFile_Click(object sender, EventArgs e)
        {
            string new_file_name = tbNewFileName.Text + ".txt";
            string path = Path.Combine(Application.StartupPath, _path_to_data, new_file_name);
            
            if(File.Exists(path))
            {
                AddLogToConsole(string.Format("File {0} already exists", new_file_name), ELogLevel.Error);
                return;
            }

            File.WriteAllText(path, "", Encoding.UTF8);

            lbSourceFiles.Items.Add(new_file_name);
            lbSourceFiles.SelectedIndex = lbSourceFiles.Items.Count - 1;
        }

        class CTest2
        {
            private int _int;

            public int PubProp { get; set; }

            public CTest2()
            {
                _int = 9;

                PubProp = 11;
            }
        }

        private void btnSerializeTests_Click(object sender, EventArgs e)
        {
            var serializer = new CKeySerializer(new CachedReflector());
            TestObject saved_obj = TestObject.CreateTestObject();
            CKey root = new CKey(null, "Root");

            serializer.Serialize(saved_obj, root, this);

            //var serializer = new Serializer(new CachedReflector());
            //SerializedObject serialized = serializer.Serialize(TestObject.CreateTestObject());
            ////SerializedObject serialized = serializer.Serialize(new CTest2());
            //string text = serialized.Stringify();
            //AddLogToRichText(text, Color.Green);

            //CKey root = new CKey(null, serialized.Name);
            //SaveToRoot(root, serialized);

            tvTree.Nodes.Clear();
            AddToTree(root, tvTree.Nodes);
            tvTree.ExpandAll();
        }

        void SaveToRoot(CBaseKey key, SerializedObject instance)
        {
            if (instance is SerializedAtom) SaveToRoot(key, instance as SerializedAtom);
            if (instance is SerializedAggregate) SaveToRoot(key, instance as SerializedAggregate);
            if (instance is SerializedCollection) SaveToRoot(key, instance as SerializedCollection);
        }

        void SaveToRoot(CBaseKey key, SerializedAggregate instance)
        {
            foreach (var child in instance.Children.Keys)
            {
                CKey sub_key = null;
                if (child is SerializedAtom)
                {
                    var atom = child as SerializedAtom;
                    sub_key = new CKey(key, atom.Value.ToString());
                }
                else if (child is SerializedObject)
                {
                    var ser_object = child as SerializedObject;
                    AddLogToConsole(string.Format("SerializedObject as key: {0}, previous key {1}", ser_object, key.Name), ELogLevel.Error);
                }
                else if (child is string)
                    sub_key = new CKey(key, child as string);
                else
                    AddLogToConsole(string.Format("Unknown child type {0}", child.GetType().Name), ELogLevel.Error);

                if (sub_key == null)
                    continue;

                SerializedObject child_value = instance.Children[child];
                if(child_value is SerializedAtom)
                    SaveToRoot(sub_key, child_value as SerializedAtom);
                else if (child_value is SerializedCollection)
                    SaveToRoot(sub_key, child_value as SerializedCollection);
                else
                    SaveToRoot(sub_key, child_value);
            }
        }

        void SaveToRoot(CBaseKey key, SerializedCollection collection)
        {
            int array_index = 0;
            foreach (var item in collection.Items)
            {
                if (item is SerializedAtom)
                {
                    SaveToRoot(key, item as SerializedAtom);
                }
                else
                {
                    CArrayKey arr_key = new CArrayKey(key, array_index);
                    array_index++;
                    SaveToRoot(arr_key, item);
                }
            }
        }
        
        void SaveToRoot(CBaseKey key, SerializedAtom instance)
        {
            if (instance.Value == null)
            {
                var value = new CIntValue(key, 0);
                return;
            }
            if(ReflectionHelper.IsFLOAT(instance.Value))
            {
                var value = new CFloatValue(key, Convert.ToDecimal(instance.Value));
                return;
            }
            if (ReflectionHelper.IsINT(instance.Value))
            {
                var value = new CIntValue(key, Convert.ToInt64(instance.Value));
                return;
            }
            if (ReflectionHelper.IsUINT(instance.Value))
            {
                var value = new CUIntValue(key, Convert.ToUInt64(instance.Value));
                return;
            }
            var string_value = new CStringValue(key, instance.Value.ToString());
        }
    }
}

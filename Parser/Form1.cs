using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Parser
{
    public partial class Form1 : Form, ILogPrinter
    {
        const string _path_to_data = "Data";

        string _selected_file;

        CSentenseDivider _sentenser = new CSentenseDivider();

        CLoger _loger;

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

        #endregion //LogWindow Output

        private void Form1_Load(object sender, EventArgs e)
        {
            _loger = new CLoger(this);

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

            _sentenser.ParseText(tbSourceText.Text, _loger);

            List<CTokenLine> _lines = new List<CTokenLine>();
            int ecount = 0;

            for (int i = 0; i < _sentenser.SentenseCount; i++)
            {
                CSentense sentense = _sentenser[i];

                CTokenLine tl = new CTokenLine();
                tl.Init(sentense, _loger);

                ecount += tl.ErrorCount;

                _lines.Add(tl);
            }

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < _lines.Count; i++)
            {
                sb.Append(string.Format("{0}: {1}{2}", i.ToString("D4"), _lines[i], Environment.NewLine));
            }
            
            tbResult.Text = sb.ToString();

            CKey k = CTreeBuilder.Build(_lines, _loger);

            AddToTree(k, tvTree.Nodes);
            tvTree.ExpandAll();
        }

        void AddToTree(CBaseKey key, TreeNodeCollection nc)
        {
            TreeNode tn = new TreeNode(key.GetDebugText());
            nc.Add(tn);
            for(int i = 0; i < key.ElementCount; i++)
            {
                CBaseElement el = key[i];
                if (el.GetElementType() == EElementType.Key ||
                    el.GetElementType() == EElementType.ArrayKey)
                    AddToTree(el as CBaseKey, tn.Nodes);
                else
                {
                    TreeNode etn = new TreeNode(el.GetDebugText());
                    tn.Nodes.Add(etn);
                }
            }
        }
    }
}

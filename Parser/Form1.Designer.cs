namespace Parser
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbSourceText = new System.Windows.Forms.TextBox();
            this.lbSourceFiles = new System.Windows.Forms.ListBox();
            this.tbResult = new System.Windows.Forms.TextBox();
            this.btnParse = new System.Windows.Forms.Button();
            this.rtLog = new System.Windows.Forms.RichTextBox();
            this.tvTree = new System.Windows.Forms.TreeView();
            this.tbNewFileName = new System.Windows.Forms.TextBox();
            this.btnNewFile = new System.Windows.Forms.Button();
            this.btnSerializeTests = new System.Windows.Forms.Button();
            this.btnDeserializeTest = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbSourceText
            // 
            this.tbSourceText.AcceptsReturn = true;
            this.tbSourceText.AcceptsTab = true;
            this.tbSourceText.Location = new System.Drawing.Point(12, 12);
            this.tbSourceText.Multiline = true;
            this.tbSourceText.Name = "tbSourceText";
            this.tbSourceText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbSourceText.Size = new System.Drawing.Size(859, 281);
            this.tbSourceText.TabIndex = 0;
            this.tbSourceText.Leave += new System.EventHandler(this.tbSourceText_Leave);
            // 
            // lbSourceFiles
            // 
            this.lbSourceFiles.FormattingEnabled = true;
            this.lbSourceFiles.Location = new System.Drawing.Point(877, 12);
            this.lbSourceFiles.Name = "lbSourceFiles";
            this.lbSourceFiles.Size = new System.Drawing.Size(415, 95);
            this.lbSourceFiles.TabIndex = 1;
            this.lbSourceFiles.SelectedIndexChanged += new System.EventHandler(this.lbSourceFiles_SelectedIndexChanged);
            // 
            // tbResult
            // 
            this.tbResult.Location = new System.Drawing.Point(12, 338);
            this.tbResult.Multiline = true;
            this.tbResult.Name = "tbResult";
            this.tbResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbResult.Size = new System.Drawing.Size(859, 288);
            this.tbResult.TabIndex = 0;
            // 
            // btnParse
            // 
            this.btnParse.Location = new System.Drawing.Point(12, 299);
            this.btnParse.Name = "btnParse";
            this.btnParse.Size = new System.Drawing.Size(88, 23);
            this.btnParse.TabIndex = 2;
            this.btnParse.Text = "Parse";
            this.btnParse.UseVisualStyleBackColor = true;
            this.btnParse.Click += new System.EventHandler(this.btnParse_Click);
            // 
            // rtLog
            // 
            this.rtLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtLog.HideSelection = false;
            this.rtLog.Location = new System.Drawing.Point(12, 632);
            this.rtLog.Name = "rtLog";
            this.rtLog.Size = new System.Drawing.Size(859, 187);
            this.rtLog.TabIndex = 30;
            this.rtLog.Text = "";
            // 
            // tvTree
            // 
            this.tvTree.Location = new System.Drawing.Point(877, 338);
            this.tvTree.Name = "tvTree";
            this.tvTree.Size = new System.Drawing.Size(415, 481);
            this.tvTree.TabIndex = 31;
            // 
            // tbNewFileName
            // 
            this.tbNewFileName.Location = new System.Drawing.Point(877, 113);
            this.tbNewFileName.Name = "tbNewFileName";
            this.tbNewFileName.Size = new System.Drawing.Size(216, 20);
            this.tbNewFileName.TabIndex = 32;
            // 
            // btnNewFile
            // 
            this.btnNewFile.Location = new System.Drawing.Point(1099, 111);
            this.btnNewFile.Name = "btnNewFile";
            this.btnNewFile.Size = new System.Drawing.Size(106, 23);
            this.btnNewFile.TabIndex = 33;
            this.btnNewFile.Text = "CreateNewFile";
            this.btnNewFile.UseVisualStyleBackColor = true;
            this.btnNewFile.Click += new System.EventHandler(this.btnNewFile_Click);
            // 
            // btnSerializeTests
            // 
            this.btnSerializeTests.Location = new System.Drawing.Point(1185, 270);
            this.btnSerializeTests.Name = "btnSerializeTests";
            this.btnSerializeTests.Size = new System.Drawing.Size(107, 23);
            this.btnSerializeTests.TabIndex = 34;
            this.btnSerializeTests.Text = "SerializeTests";
            this.btnSerializeTests.UseVisualStyleBackColor = true;
            this.btnSerializeTests.Click += new System.EventHandler(this.btnSerializeTests_Click);
            // 
            // btnDeserializeTest
            // 
            this.btnDeserializeTest.Location = new System.Drawing.Point(1185, 299);
            this.btnDeserializeTest.Name = "btnDeserializeTest";
            this.btnDeserializeTest.Size = new System.Drawing.Size(107, 23);
            this.btnDeserializeTest.TabIndex = 35;
            this.btnDeserializeTest.Text = "Deserialize Test";
            this.btnDeserializeTest.UseVisualStyleBackColor = true;
            this.btnDeserializeTest.Click += new System.EventHandler(this.btnDeserializeTest_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1304, 831);
            this.Controls.Add(this.btnDeserializeTest);
            this.Controls.Add(this.btnSerializeTests);
            this.Controls.Add(this.btnNewFile);
            this.Controls.Add(this.tbNewFileName);
            this.Controls.Add(this.tvTree);
            this.Controls.Add(this.rtLog);
            this.Controls.Add(this.btnParse);
            this.Controls.Add(this.lbSourceFiles);
            this.Controls.Add(this.tbResult);
            this.Controls.Add(this.tbSourceText);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbSourceText;
        private System.Windows.Forms.ListBox lbSourceFiles;
        private System.Windows.Forms.TextBox tbResult;
        private System.Windows.Forms.Button btnParse;
        private System.Windows.Forms.RichTextBox rtLog;
        private System.Windows.Forms.TreeView tvTree;
        private System.Windows.Forms.TextBox tbNewFileName;
        private System.Windows.Forms.Button btnNewFile;
        private System.Windows.Forms.Button btnSerializeTests;
        private System.Windows.Forms.Button btnDeserializeTest;
    }
}


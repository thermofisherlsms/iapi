namespace Thermo.IAPI.Examples
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.txtMethodPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnDatabase = new System.Windows.Forms.Button();
            this.txtDB = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tableprecursorBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.methoddbDataSetBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.methoddbDataSet = new Thermo.IAPI.Examples.methoddbDataSet();
            this.label3 = new System.Windows.Forms.Label();
            this.upDownThreshold = new System.Windows.Forms.NumericUpDown();
            this.btnStart = new System.Windows.Forms.Button();
            this.table_precursorTableAdapter = new Thermo.IAPI.Examples.methoddbDataSetTableAdapters.Table_precursorTableAdapter();
            this.label4 = new System.Windows.Forms.Label();
            this.lblIsConnected = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.precursorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tableprecursorBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.methoddbDataSetBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.methoddbDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownThreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 102);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select Method";
            // 
            // txtMethodPath
            // 
            this.txtMethodPath.Location = new System.Drawing.Point(121, 102);
            this.txtMethodPath.Name = "txtMethodPath";
            this.txtMethodPath.Size = new System.Drawing.Size(472, 20);
            this.txtMethodPath.TabIndex = 1;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(599, 101);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(40, 23);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnDatabase
            // 
            this.btnDatabase.Location = new System.Drawing.Point(599, 137);
            this.btnDatabase.Name = "btnDatabase";
            this.btnDatabase.Size = new System.Drawing.Size(40, 23);
            this.btnDatabase.TabIndex = 5;
            this.btnDatabase.Text = "...";
            this.btnDatabase.UseVisualStyleBackColor = true;
            this.btnDatabase.Click += new System.EventHandler(this.btnDatabase_Click);
            // 
            // txtDB
            // 
            this.txtDB.Location = new System.Drawing.Point(121, 138);
            this.txtDB.Name = "txtDB";
            this.txtDB.Size = new System.Drawing.Size(472, 20);
            this.txtDB.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 138);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Select Database";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.precursorDataGridViewTextBoxColumn});
            this.dataGridView1.DataSource = this.tableprecursorBindingSource;
            this.dataGridView1.Location = new System.Drawing.Point(15, 236);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(181, 278);
            this.dataGridView1.TabIndex = 6;
            // 
            // tableprecursorBindingSource
            // 
            this.tableprecursorBindingSource.DataMember = "Table_precursor";
            this.tableprecursorBindingSource.DataSource = this.methoddbDataSetBindingSource;
            // 
            // methoddbDataSetBindingSource
            // 
            this.methoddbDataSetBindingSource.DataSource = this.methoddbDataSet;
            this.methoddbDataSetBindingSource.Position = 0;
            // 
            // methoddbDataSet
            // 
            this.methoddbDataSet.DataSetName = "methoddbDataSet";
            this.methoddbDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 541);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(114, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Dot Product Threshold";
            // 
            // upDownThreshold
            // 
            this.upDownThreshold.DecimalPlaces = 1;
            this.upDownThreshold.ImeMode = System.Windows.Forms.ImeMode.On;
            this.upDownThreshold.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.upDownThreshold.Location = new System.Drawing.Point(144, 541);
            this.upDownThreshold.Name = "upDownThreshold";
            this.upDownThreshold.Size = new System.Drawing.Size(78, 20);
            this.upDownThreshold.TabIndex = 8;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(565, 541);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 9;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // table_precursorTableAdapter
            // 
            this.table_precursorTableAdapter.ClearBeforeFill = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "IsConnected";
            // 
            // lblIsConnected
            // 
            this.lblIsConnected.AutoSize = true;
            this.lblIsConnected.Location = new System.Drawing.Point(88, 22);
            this.lblIsConnected.Name = "lblIsConnected";
            this.lblIsConnected.Size = new System.Drawing.Size(0, 13);
            this.lblIsConnected.TabIndex = 11;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(247, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 12;
            this.button1.Text = "Connect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(566, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 13;
            this.button2.Text = "Disconnect";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(328, 12);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 14;
            this.button3.Text = "Start Scan";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // precursorDataGridViewTextBoxColumn
            // 
            this.precursorDataGridViewTextBoxColumn.DataPropertyName = "Precursor";
            this.precursorDataGridViewTextBoxColumn.HeaderText = "Precursor";
            this.precursorDataGridViewTextBoxColumn.Name = "precursorDataGridViewTextBoxColumn";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(653, 578);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lblIsConnected);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.upDownThreshold);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btnDatabase);
            this.Controls.Add(this.txtDB);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtMethodPath);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Fusion - Method Filter Example";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tableprecursorBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.methoddbDataSetBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.methoddbDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownThreshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtMethodPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnDatabase;
        private System.Windows.Forms.TextBox txtDB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown upDownThreshold;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.BindingSource methoddbDataSetBindingSource;
        private methoddbDataSet methoddbDataSet;
        private System.Windows.Forms.BindingSource tableprecursorBindingSource;
        private methoddbDataSetTableAdapters.Table_precursorTableAdapter table_precursorTableAdapter;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblIsConnected;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.DataGridViewTextBoxColumn precursorDataGridViewTextBoxColumn;
    }
}


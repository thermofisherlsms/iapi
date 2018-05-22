namespace SimpleGuiExample
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
			this.ButtonInterfaceTest = new System.Windows.Forms.Button();
			this.ButtonSprayVoltage = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.TextBoxStatus = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// ButtonInterfaceTest
			// 
			this.ButtonInterfaceTest.Location = new System.Drawing.Point(12, 12);
			this.ButtonInterfaceTest.Name = "ButtonInterfaceTest";
			this.ButtonInterfaceTest.Size = new System.Drawing.Size(126, 23);
			this.ButtonInterfaceTest.TabIndex = 0;
			this.ButtonInterfaceTest.Text = "Interface Presence";
			this.toolTip1.SetToolTip(this.ButtonInterfaceTest, "Tests whether the interface to the instrument is accessible. Exactive Series soft" +
        "ware needs to be installed.");
			this.ButtonInterfaceTest.UseVisualStyleBackColor = true;
			this.ButtonInterfaceTest.Click += new System.EventHandler(this.ButtonInterfaceTest_Click);
			// 
			// ButtonSprayVoltage
			// 
			this.ButtonSprayVoltage.Location = new System.Drawing.Point(144, 12);
			this.ButtonSprayVoltage.Name = "ButtonSprayVoltage";
			this.ButtonSprayVoltage.Size = new System.Drawing.Size(140, 23);
			this.ButtonSprayVoltage.TabIndex = 1;
			this.ButtonSprayVoltage.Text = "Set Spray voltage to 100";
			this.ButtonSprayVoltage.UseVisualStyleBackColor = true;
			this.ButtonSprayVoltage.Click += new System.EventHandler(this.ButtonSprayVoltage_Click);
			// 
			// TextBoxStatus
			// 
			this.TextBoxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextBoxStatus.BackColor = System.Drawing.Color.Snow;
			this.TextBoxStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.TextBoxStatus.Location = new System.Drawing.Point(12, 51);
			this.TextBoxStatus.Multiline = true;
			this.TextBoxStatus.Name = "TextBoxStatus";
			this.TextBoxStatus.ReadOnly = true;
			this.TextBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.TextBoxStatus.Size = new System.Drawing.Size(485, 199);
			this.TextBoxStatus.TabIndex = 2;
			this.TextBoxStatus.Text = "No action performed so far.";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(509, 262);
			this.Controls.Add(this.TextBoxStatus);
			this.Controls.Add(this.ButtonSprayVoltage);
			this.Controls.Add(this.ButtonInterfaceTest);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button ButtonInterfaceTest;
		private System.Windows.Forms.Button ButtonSprayVoltage;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.TextBox TextBoxStatus;
	}
}


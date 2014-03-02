namespace GraphMapStressTester
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
            this.nodesBox = new System.Windows.Forms.TextBox();
            this.ratioBox = new System.Windows.Forms.TextBox();
            this.goButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.testOptionBox = new System.Windows.Forms.ComboBox();
            this.noDoorsBox = new System.Windows.Forms.TextBox();
            this.noCluesBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.seedBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.visualiseBox = new System.Windows.Forms.CheckBox();
            this.iterationBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // nodesBox
            // 
            this.nodesBox.Location = new System.Drawing.Point(134, 31);
            this.nodesBox.Name = "nodesBox";
            this.nodesBox.Size = new System.Drawing.Size(100, 20);
            this.nodesBox.TabIndex = 0;
            this.nodesBox.Text = "20";
            // 
            // ratioBox
            // 
            this.ratioBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ratioBox.Location = new System.Drawing.Point(134, 92);
            this.ratioBox.Name = "ratioBox";
            this.ratioBox.Size = new System.Drawing.Size(100, 20);
            this.ratioBox.TabIndex = 1;
            this.ratioBox.Text = "0.5";
            // 
            // goButton
            // 
            this.goButton.Location = new System.Drawing.Point(134, 145);
            this.goButton.Name = "goButton";
            this.goButton.Size = new System.Drawing.Size(75, 23);
            this.goButton.TabIndex = 2;
            this.goButton.Text = "Go";
            this.goButton.UseVisualStyleBackColor = true;
            this.goButton.Click += new System.EventHandler(this.goButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Number of Nodes";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Branching Ratio";
            // 
            // testOptionBox
            // 
            this.testOptionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.testOptionBox.Items.AddRange(new object[] {
            "GraphGenerator",
            "DoorAndClue"});
            this.testOptionBox.Location = new System.Drawing.Point(113, 192);
            this.testOptionBox.Name = "testOptionBox";
            this.testOptionBox.Size = new System.Drawing.Size(121, 21);
            this.testOptionBox.TabIndex = 5;
            // 
            // noDoorsBox
            // 
            this.noDoorsBox.Location = new System.Drawing.Point(134, 232);
            this.noDoorsBox.Name = "noDoorsBox";
            this.noDoorsBox.Size = new System.Drawing.Size(100, 20);
            this.noDoorsBox.TabIndex = 6;
            this.noDoorsBox.Text = "20";
            // 
            // noCluesBox
            // 
            this.noCluesBox.Location = new System.Drawing.Point(134, 269);
            this.noCluesBox.Name = "noCluesBox";
            this.noCluesBox.Size = new System.Drawing.Size(100, 20);
            this.noCluesBox.TabIndex = 7;
            this.noCluesBox.Text = "3";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(38, 235);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Number of Doors";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(38, 269);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(85, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Number of Clues";
            // 
            // seedBox
            // 
            this.seedBox.Location = new System.Drawing.Point(133, 309);
            this.seedBox.Name = "seedBox";
            this.seedBox.Size = new System.Drawing.Size(100, 20);
            this.seedBox.TabIndex = 10;
            this.seedBox.Text = "111111";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(38, 312);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Random Seed";
            // 
            // visualiseBox
            // 
            this.visualiseBox.AutoSize = true;
            this.visualiseBox.Checked = true;
            this.visualiseBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.visualiseBox.Location = new System.Drawing.Point(41, 347);
            this.visualiseBox.Name = "visualiseBox";
            this.visualiseBox.Size = new System.Drawing.Size(67, 17);
            this.visualiseBox.TabIndex = 12;
            this.visualiseBox.Text = "Visualise";
            this.visualiseBox.UseVisualStyleBackColor = true;
            // 
            // iterationBox
            // 
            this.iterationBox.Location = new System.Drawing.Point(133, 380);
            this.iterationBox.Name = "iterationBox";
            this.iterationBox.Size = new System.Drawing.Size(100, 20);
            this.iterationBox.TabIndex = 13;
            this.iterationBox.Text = "1";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(38, 383);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Iterations";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 440);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.iterationBox);
            this.Controls.Add(this.visualiseBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.seedBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.noCluesBox);
            this.Controls.Add(this.noDoorsBox);
            this.Controls.Add(this.testOptionBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.goButton);
            this.Controls.Add(this.ratioBox);
            this.Controls.Add(this.nodesBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox nodesBox;
        private System.Windows.Forms.TextBox ratioBox;
        private System.Windows.Forms.Button goButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox testOptionBox;
        private System.Windows.Forms.TextBox noDoorsBox;
        private System.Windows.Forms.TextBox noCluesBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox seedBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox visualiseBox;
        private System.Windows.Forms.TextBox iterationBox;
        private System.Windows.Forms.Label label6;

    }
}


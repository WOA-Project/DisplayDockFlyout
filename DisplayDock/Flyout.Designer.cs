namespace DisplayDockFlyout
{
    partial class Flyout
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
            this.windowsXamlHost1 = new Microsoft.Toolkit.Forms.UI.XamlHost.WindowsXamlHost();
            this.SuspendLayout();
            // 
            // windowsXamlHost1
            // 
            this.windowsXamlHost1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.windowsXamlHost1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.windowsXamlHost1.InitialTypeName = null;
            this.windowsXamlHost1.Location = new System.Drawing.Point(0, 0);
            this.windowsXamlHost1.Margin = new System.Windows.Forms.Padding(0);
            this.windowsXamlHost1.Name = "windowsXamlHost1";
            this.windowsXamlHost1.Size = new System.Drawing.Size(360, 582);
            this.windowsXamlHost1.TabIndex = 0;
            // 
            // Flyout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 582);
            this.Controls.Add(this.windowsXamlHost1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Flyout";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Flyout";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        private Microsoft.Toolkit.Forms.UI.XamlHost.WindowsXamlHost windowsXamlHost1;
        #endregion
    }
}
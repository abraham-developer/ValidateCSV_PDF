namespace ValidateCSV_PDF
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtResults = new System.Windows.Forms.TextBox();
            this.btnSelectCSV = new System.Windows.Forms.Button();
            this.btnPasteTrackingNumbers = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtResults
            // 
            this.txtResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtResults.Location = new System.Drawing.Point(12, 50);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResults.Size = new System.Drawing.Size(460, 250);
            this.txtResults.TabIndex = 2;
            // 
            // btnSelectCSV
            // 
            this.btnSelectCSV.Location = new System.Drawing.Point(12, 12);
            this.btnSelectCSV.Name = "btnSelectCSV";
            this.btnSelectCSV.Size = new System.Drawing.Size(150, 30);
            this.btnSelectCSV.TabIndex = 0;
            this.btnSelectCSV.Text = "Select CSV File";
            this.btnSelectCSV.UseVisualStyleBackColor = true;
            this.btnSelectCSV.Click += new System.EventHandler(this.btnSelectCSV_Click);
            // 
            // btnPasteTrackingNumbers
            // 
            this.btnPasteTrackingNumbers.Location = new System.Drawing.Point(168, 12);
            this.btnPasteTrackingNumbers.Name = "btnPasteTrackingNumbers";
            this.btnPasteTrackingNumbers.Size = new System.Drawing.Size(180, 30);
            this.btnPasteTrackingNumbers.TabIndex = 1;
            this.btnPasteTrackingNumbers.Text = "Paste Tracking Numbers";
            this.btnPasteTrackingNumbers.UseVisualStyleBackColor = true;
            this.btnPasteTrackingNumbers.Click += new System.EventHandler(this.btnPasteTrackingNumbers_Click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.btnPasteTrackingNumbers);
            this.Controls.Add(this.txtResults);
            this.Controls.Add(this.btnSelectCSV);
            this.Name = "Form1";
            this.Text = "Tracking Number PDF Validator";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}


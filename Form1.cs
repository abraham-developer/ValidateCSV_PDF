using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;


namespace ValidateCSV_PDF
{
    public partial class Form1 : Form
    {
        private TextBox txtResults;
        private Button btnSelectCSV;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnSelectCSV_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV Files (*.csv)|*.csv";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string csvFilePath = openFileDialog.FileName;
                    ProcessCSVFile(csvFilePath);
                }
            }
        }

        private void ProcessCSVFile(string csvFilePath)
        {
            try
            {
                // Get the directory of the CSV file
                string baseDirectory = Path.GetDirectoryName(csvFilePath);

                // Create a folder to store found PDFs
                string outputFolder = Path.Combine(baseDirectory, "FoundPDFs");
                Directory.CreateDirectory(outputFolder);

                // List to store processing results
                List<string> processResults = new List<string>();

                // Use TextFieldParser for robust CSV parsing
                using (TextFieldParser parser = new TextFieldParser(csvFilePath, Encoding.UTF8))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    parser.HasFieldsEnclosedInQuotes = true;

                    // Skip header if exists
                    if (!parser.EndOfData)
                        parser.ReadFields();

                    // Process each row
                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        if (fields != null && fields.Length >= 2)
                        {
                            // Remove quotes from tracking number
                            string trackingNumber = fields[0].Trim('"');

                            // Check if PDF exists
                            string pdfPath = Path.Combine(baseDirectory, $"{trackingNumber}.pdf");
                            if (File.Exists(pdfPath))
                            {
                                // Copy PDF to output folder
                                string destinationPath = Path.Combine(outputFolder, $"{trackingNumber}.pdf");
                                File.Copy(pdfPath, destinationPath, true);
                                processResults.Add($"Found PDF for Tracking Number: {trackingNumber}");
                            }
                            else
                            {
                                processResults.Add($"No PDF found for Tracking Number: {trackingNumber}");
                            }
                        }
                    }
                }

                // Display results
                txtResults.Clear();
                txtResults.Lines = processResults.ToArray();

                MessageBox.Show($"Processing complete. {processResults.Count} tracking numbers processed.\nFound PDFs saved in: {outputFolder}",
                    "CSV Processing", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing CSV: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.txtResults = new TextBox();
            this.btnSelectCSV = new Button();

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
            this.txtResults.Size = new System.Drawing.Size(460, 300);

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
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.txtResults);
            this.Controls.Add(this.btnSelectCSV);
            this.Name = "Form1";
            this.Text = "Tracking Number PDF Validator";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

namespace ValidateCSV_PDF
{
    public partial class Form1 : Form
    {
        private TextBox txtResults;
        private Button btnSelectCSV;
        private Button btnPasteTrackingNumbers;
        private TextBox txtTrackingNumbers;

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

        private void btnPasteTrackingNumbers_Click(object sender, EventArgs e)
        {
            // Show folder dialog to select base directory for storing PDFs
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select the folder where you want to store PDFs";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedDirectory = folderDialog.SelectedPath;

                    // Now, process the pasted tracking numbers and create folders in the selected directory
                    using (Form inputForm = new Form())
                    {
                        inputForm.Text = "Paste Tracking Numbers";
                        inputForm.Size = new System.Drawing.Size(400, 300);

                        TextBox txtInput = new TextBox
                        {
                            Multiline = true,
                            Dock = DockStyle.Fill,
                            ScrollBars = ScrollBars.Vertical
                        };
                        inputForm.Controls.Add(txtInput);

                        Button btnProcess = new Button
                        {
                            Text = "Process Tracking Numbers",
                            Dock = DockStyle.Bottom
                        };
                        inputForm.Controls.Add(btnProcess);

                        btnProcess.Click += (s, ev) =>
                        {
                            ProcessPastedTrackingNumbers(txtInput.Text, selectedDirectory);
                            inputForm.Close();
                        };

                        inputForm.ShowDialog();
                    }
                }
            }
        }

        private void ProcessPastedTrackingNumbers(string trackingNumbersText, string baseDirectory)
        {
            try
            {
                // Create folders to store found and not found PDFs in the selected directory
                string foundPDFsFolder = Path.Combine(baseDirectory, "FoundPDFs");
                string notFoundPDFsFolder = Path.Combine(baseDirectory, "NotFoundPDFs");
                Directory.CreateDirectory(foundPDFsFolder);
                Directory.CreateDirectory(notFoundPDFsFolder);

                // List to store processing results
                List<string> processResults = new List<string>();

                // Split the input text into tracking numbers
                string[] trackingNumbers = trackingNumbersText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Process each tracking number
                foreach (string trackingNumber in trackingNumbers)
                {
                    // Trim any whitespace and quotes
                    string cleanTrackingNumber = trackingNumber.Trim().Trim('"');

                    // Check if PDF exists
                    string pdfPath = Path.Combine(baseDirectory, $"{cleanTrackingNumber}.pdf");
                    if (File.Exists(pdfPath))
                    {
                        // Copy PDF to found PDFs folder
                        string destinationPath = Path.Combine(foundPDFsFolder, $"{cleanTrackingNumber}.pdf");
                        File.Copy(pdfPath, destinationPath, true);
                        processResults.Add($"Found PDF for Tracking Number: {cleanTrackingNumber}");
                    }
                    else
                    {
                        // Check all PDFs in base directory
                        bool pdfFound = false;
                        string[] allPdfFiles = Directory.GetFiles(baseDirectory, "*.pdf");
                        foreach (string pdf in allPdfFiles)
                        {
                            string pdfFileName = Path.GetFileNameWithoutExtension(pdf);
                            if (pdfFileName.Contains(cleanTrackingNumber))
                            {
                                // Copy PDF to found PDFs folder
                                string destinationPath = Path.Combine(foundPDFsFolder, $"{pdfFileName}.pdf");
                                File.Copy(pdf, destinationPath, true);
                                processResults.Add($"Found PDF for Tracking Number: {cleanTrackingNumber} (partial match)");
                                pdfFound = true;
                                break;
                            }
                        }

                        if (!pdfFound)
                        {
                            processResults.Add($"No PDF found for Tracking Number: {cleanTrackingNumber}");

                            // Check if there's a PDF with tracking number in base directory
                            string[] similarPdfs = Directory.GetFiles(baseDirectory, $"*{cleanTrackingNumber}*.pdf");
                            foreach (string similarPdf in similarPdfs)
                            {
                                string destinationPath = Path.Combine(notFoundPDFsFolder, Path.GetFileName(similarPdf));
                                File.Copy(similarPdf, destinationPath, true);
                                processResults.Add($"Moved similar PDF to Not Found folder: {Path.GetFileName(similarPdf)}");
                            }
                        }
                    }
                }

                // Display results
                txtResults.Clear();
                txtResults.Lines = processResults.ToArray();

                MessageBox.Show($"Processing complete.\n" +
                    $"Total tracking numbers: {processResults.Count}\n" +
                    $"Found PDFs saved in: {foundPDFsFolder}\n" +
                    $"Not Found PDFs saved in: {notFoundPDFsFolder}",
                    "Manual Tracking Numbers Processing", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing tracking numbers: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProcessCSVFile(string csvFilePath)
        {
            try
            {
                // Get the directory of the CSV file
                string baseDirectory = Path.GetDirectoryName(csvFilePath);

                // Create folders to store found and not found PDFs in the selected directory
                string foundPDFsFolder = Path.Combine(baseDirectory, "FoundPDFs");
                string notFoundPDFsFolder = Path.Combine(baseDirectory, "NotFoundPDFs");
                Directory.CreateDirectory(foundPDFsFolder);
                Directory.CreateDirectory(notFoundPDFsFolder);

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
                                // Copy PDF to found PDFs folder
                                string destinationPath = Path.Combine(foundPDFsFolder, $"{trackingNumber}.pdf");
                                File.Copy(pdfPath, destinationPath, true);
                                processResults.Add($"Found PDF for Tracking Number: {trackingNumber}");
                            }
                            else
                            {
                                // Check all PDFs in base directory
                                bool pdfFound = false;
                                string[] allPdfFiles = Directory.GetFiles(baseDirectory, "*.pdf");
                                foreach (string pdf in allPdfFiles)
                                {
                                    string pdfFileName = Path.GetFileNameWithoutExtension(pdf);
                                    if (pdfFileName.Contains(trackingNumber))
                                    {
                                        // Copy PDF to found PDFs folder
                                        string destinationPath = Path.Combine(foundPDFsFolder, $"{pdfFileName}.pdf");
                                        File.Copy(pdf, destinationPath, true);
                                        processResults.Add($"Found PDF for Tracking Number: {trackingNumber} (partial match)");
                                        pdfFound = true;
                                        break;
                                    }
                                }

                                if (!pdfFound)
                                {
                                    // Try to find similar PDFs with partial matching
                                    processResults.Add($"No PDF found for Tracking Number: {trackingNumber}");

                                    // Check if there's a PDF with tracking number in base directory
                                    string[] similarPdfs = Directory.GetFiles(baseDirectory, $"*{trackingNumber}*.pdf");
                                    foreach (string similarPdf in similarPdfs)
                                    {
                                        string destinationPath = Path.Combine(notFoundPDFsFolder, Path.GetFileName(similarPdf));
                                        File.Copy(similarPdf, destinationPath, true);
                                        processResults.Add($"Moved similar PDF to Not Found folder: {Path.GetFileName(similarPdf)}");
                                    }
                                }
                            }
                        }
                    }
                }

                // Display results
                txtResults.Clear();
                txtResults.Lines = processResults.ToArray();

                MessageBox.Show($"Processing complete.\n" +
                    $"Total tracking numbers: {processResults.Count}\n" +
                    $"Found PDFs saved in: {foundPDFsFolder}\n" +
                    $"Not Found PDFs saved in: {notFoundPDFsFolder}",
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
            this.btnPasteTrackingNumbers = new Button();

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
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}

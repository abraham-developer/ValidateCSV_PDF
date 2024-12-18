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
            SetupComponents();
        }

        private void Form1_Load(object sender, EventArgs e)
        { 
        
        }

        private void SetupComponents()
        {
            // Configurar el formulario principal
            this.Text = "PDF Tracking Validator";
            this.Size = new System.Drawing.Size(800, 600);

            // Crear y configurar btnSelectCSV
            btnSelectCSV = new Button
            {
                Text = "Seleccionar CSV",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(150, 30)
            };
            btnSelectCSV.Click += btnSelectCSV_Click;

            // Crear y configurar btnPasteTrackingNumbers
            btnPasteTrackingNumbers = new Button
            {
                Text = "Pegar Tracking Numbers",
                Location = new System.Drawing.Point(190, 20),
                Size = new System.Drawing.Size(150, 30)
            };
            btnPasteTrackingNumbers.Click += btnPasteTrackingNumbers_Click;

            // Crear y configurar txtResults
            txtResults = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new System.Drawing.Point(20, 70),
                Size = new System.Drawing.Size(740, 470),
                ReadOnly = true
            };

            // Agregar controles al formulario
            this.Controls.Add(btnSelectCSV);
            this.Controls.Add(btnPasteTrackingNumbers);
            this.Controls.Add(txtResults);
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
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select the folder where you want to store PDFs";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedDirectory = folderDialog.SelectedPath;

                    using (Form inputForm = new Form())
                    {
                        inputForm.Text = "Paste Tracking Numbers";
                        inputForm.Size = new System.Drawing.Size(800, 600);

                        // Usar RichTextBox para mejor rendimiento
                        RichTextBox txtInput = new RichTextBox
                        {
                            Multiline = true,
                            Dock = DockStyle.Fill,
                            ScrollBars = RichTextBoxScrollBars.Vertical,
                            WordWrap = false,
                            AcceptsTab = true,
                            MaxLength = 0
                        };
                        inputForm.Controls.Add(txtInput);

                        // Agregar etiqueta de estado
                        Label lblStatus = new Label
                        {
                            Dock = DockStyle.Bottom,
                            Height = 20,
                            Text = "Listo para procesar tracking numbers"
                        };
                        inputForm.Controls.Add(lblStatus);

                        // Agregar barra de progreso
                        ProgressBar progressBar = new ProgressBar
                        {
                            Dock = DockStyle.Bottom,
                            Height = 20,
                            Style = ProgressBarStyle.Blocks
                        };
                        inputForm.Controls.Add(progressBar);

                        Button btnProcess = new Button
                        {
                            Text = "Procesar Tracking Numbers",
                            Dock = DockStyle.Bottom,
                            Height = 30
                        };
                        inputForm.Controls.Add(btnProcess);

                        btnProcess.Click += async (s, ev) =>
                        {
                            btnProcess.Enabled = false;
                            txtInput.ReadOnly = true;
                            lblStatus.Text = "Procesando tracking numbers...";

                            try
                            {
                                string text = txtInput.Text;
                                var trackingNumbers = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                                progressBar.Maximum = trackingNumbers.Length;
                                progressBar.Value = 0;

                                const int batchSize = 1000;
                                for (int i = 0; i < trackingNumbers.Length; i += batchSize)
                                {
                                    var batch = trackingNumbers.Skip(i).Take(batchSize);
                                    await System.Threading.Tasks.Task.Run(() =>
                                    {
                                        ProcessTrackingNumbersBatch(batch, selectedDirectory);
                                    });

                                    progressBar.Value = Math.Min(i + batchSize, trackingNumbers.Length);
                                    lblStatus.Text = $"Procesados {progressBar.Value} de {trackingNumbers.Length} tracking numbers...";
                                    Application.DoEvents();
                                }

                                MessageBox.Show($"Procesamiento completado.\nTotal de tracking numbers procesados: {trackingNumbers.Length}",
                                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error procesando tracking numbers: {ex.Message}",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            finally
                            {
                                inputForm.Close();
                            }
                        };

                        inputForm.ShowDialog();
                    }
                }
            }
        }

        private void ProcessTrackingNumbersBatch(IEnumerable<string> trackingNumbers, string baseDirectory)
        {
            string foundPDFsFolder = Path.Combine(baseDirectory, "FoundPDFs");
            string notFoundPDFsFolder = Path.Combine(baseDirectory, "NotFoundPDFs");

            // Asegurar que existan las carpetas
            Directory.CreateDirectory(foundPDFsFolder);
            Directory.CreateDirectory(notFoundPDFsFolder);

            foreach (string trackingNumber in trackingNumbers)
            {
                string cleanTrackingNumber = trackingNumber.Trim().Trim('"');
                string pdfPath = Path.Combine(baseDirectory, $"{cleanTrackingNumber}.pdf");

                try
                {
                    if (File.Exists(pdfPath))
                    {
                        string destinationPath = Path.Combine(foundPDFsFolder, $"{cleanTrackingNumber}.pdf");
                        File.Copy(pdfPath, destinationPath, true);
                        UpdateResults($"Encontrado PDF para Tracking Number: {cleanTrackingNumber}");
                    }
                    else
                    {
                        bool pdfFound = false;
                        string[] allPdfFiles = Directory.GetFiles(baseDirectory, "*.pdf");
                        foreach (string pdf in allPdfFiles)
                        {
                            string pdfFileName = Path.GetFileNameWithoutExtension(pdf);
                            if (pdfFileName.Contains(cleanTrackingNumber))
                            {
                                string destinationPath = Path.Combine(foundPDFsFolder, $"{pdfFileName}.pdf");
                                File.Copy(pdf, destinationPath, true);
                                UpdateResults($"Encontrado PDF para Tracking Number: {cleanTrackingNumber} (coincidencia parcial)");
                                pdfFound = true;
                                break;
                            }
                        }

                        if (!pdfFound)
                        {
                            UpdateResults($"No se encontró PDF para Tracking Number: {cleanTrackingNumber}");
                            string[] similarPdfs = Directory.GetFiles(baseDirectory, $"*{cleanTrackingNumber}*.pdf");
                            foreach (string similarPdf in similarPdfs)
                            {
                                string destinationPath = Path.Combine(notFoundPDFsFolder, Path.GetFileName(similarPdf));
                                File.Copy(similarPdf, destinationPath, true);
                                UpdateResults($"Movido PDF similar a carpeta Not Found: {Path.GetFileName(similarPdf)}");
                            }
                        }
                    }
                }
                catch (IOException ex)
                {
                    UpdateResults($"Error procesando {cleanTrackingNumber}: {ex.Message}");
                    continue;
                }
            }
        }

        private void UpdateResults(string message)
        {
            if (txtResults.InvokeRequired)
            {
                txtResults.Invoke(new Action(() => UpdateResults(message)));
            }
            else
            {
                txtResults.AppendText(message + Environment.NewLine);
            }
        }

        private void ProcessCSVFile(string csvFilePath)
        {
            try
            {
                string baseDirectory = Path.GetDirectoryName(csvFilePath);
                string foundPDFsFolder = Path.Combine(baseDirectory, "FoundPDFs");
                string notFoundPDFsFolder = Path.Combine(baseDirectory, "NotFoundPDFs");

                Directory.CreateDirectory(foundPDFsFolder);
                Directory.CreateDirectory(notFoundPDFsFolder);

                List<string> processResults = new List<string>();

                using (TextFieldParser parser = new TextFieldParser(csvFilePath, Encoding.UTF8))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    parser.HasFieldsEnclosedInQuotes = true;

                    if (!parser.EndOfData)
                        parser.ReadFields();

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        if (fields != null && fields.Length >= 2)
                        {
                            string trackingNumber = fields[0].Trim('"');
                            ProcessTrackingNumbersBatch(new[] { trackingNumber }, baseDirectory);
                        }
                    }
                }

                MessageBox.Show($"Procesamiento completado.\n" +
                    $"PDFs encontrados guardados en: {foundPDFsFolder}\n" +
                    $"PDFs no encontrados guardados en: {notFoundPDFsFolder}",
                    "Procesamiento CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error procesando CSV: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
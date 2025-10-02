using System.ComponentModel;
using SerialNumberOCR.Services;
using SerialNumberOCR.Models;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;

namespace SerialNumberOCR.Forms;


public partial class MainForm : Form
{
    private OCRProcessor ocrProcessor;
    private ImageGenerator imageGenerator;
    private DatasetManager datasetManager;
    private Dataset dataset;

    private Button btnGenerateImages;
    private Button btnProcessImages;
    private Button btnLoadDataset;
    private Button btnSaveDataset;
    private TextBox txtSerialNumber;
    private NumericUpDown numImageCount;
    private ListBox lstResults;
    private PictureBox picPreview;
    private Label lblStatus;

    public MainForm()
    {
        InitializeComponent();
        InitializeService();
        LoadDataset();
    }
    
    private void InitializeService()
    {
        try
        {
            ocrProcessor = new OCRProcessor();
            imageGenerator = new ImageGenerator();
            datasetManager = new DatasetManager();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void LoadDataset()
    {
        try
        {
            dataset = datasetManager.LoadDataset();
            UpdateResultsList();
            lblStatus.Text = $"Dataset loaded with {dataset.SerialNumbers.Count} entries";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading dataset: {ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            dataset = new Dataset();
        }
    }
    
    //Initialize Form Appearance
    private void InitializeComponent()
    {
        this.Size = new Size(800, 600); //Windows size
        this.Text = "Serial Number OCR Detection"; //Title
        this.StartPosition = FormStartPosition.CenterScreen; //Starting point
        
        PropertiesControl();
        SetupEventHandlers();
        AddControlToFOrm();
    }

    private void PropertiesControl()
    {
        btnGenerateImages = new Button
        {
            Text = "Generate Images",
            Location = new Point(10, 10),
            Size = new Size(120, 30)
        };
        btnProcessImages = new Button
        {
            Text = "Process Images",
            Location = new Point(140, 10),
            Size = new Size(120, 30)
        };
        btnLoadDataset = new Button
        {
            Text = "Load Dataset",
            Location = new Point(270, 10),
            Size = new Size(120, 30)
        };
        btnSaveDataset = new Button
        {
            Text = "Save Dataset",
            Location = new Point(400, 10),
            Size = new Size(120, 30)
        };

        txtSerialNumber = new TextBox
        {
            Location = new Point(10, 70),
            Size = new Size(200, 25),
            Text = "SN00123"
        };
        numImageCount = new NumericUpDown
        {
            Location = new Point(220, 70),
            Size = new Size(80, 25),
            Value = 10,
            Minimum = 1,
            Maximum = 100
        };

        lstResults = new ListBox
        {
            Location = new Point(10, 110),
            Size = new Size(400, 300),
        };
        picPreview = new PictureBox
        {
            Location = new Point(420, 110),
            Size = new Size(350, 300),
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom
        };

        lblStatus = new Label
        {
            Location = new Point(10, 420),
            Size = new Size(600, 25),
            Text = "Ready!"
        };
    }
    
    //Event handler button 
    private void SetupEventHandlers()
    {
        btnGenerateImages.Click += GenerateImagesOperation;
        btnProcessImages.Click += ProcessImagesOperation;
        btnLoadDataset.Click += LoadDataOperation;
        btnSaveDataset.Click += SaveDataOperation;
        lstResults.SelectedIndexChanged += SelectChangingIndex;
    }

    private void AddControlToFOrm()
    {
        this.Controls.AddRange(new Control[]
        {
            btnGenerateImages, 
            btnProcessImages, 
            btnLoadDataset,
            btnSaveDataset,
            txtSerialNumber,
            numImageCount,
            lstResults,
            picPreview,
            lblStatus
        });

        this.Controls.Add(new Label
        {
            Text = "Serial Number",
            Location = new Point(10, 50),
            Size = new Size(100, 20)
        });
        
        this.Controls.Add(new Label
        {
            Text = "Image Count",
            Location = new Point(220, 50),
            Size = new Size(100, 20)
        });
    }
    
    //Starting bottom event handler setup
    private void GenerateImagesOperation(object sender, EventArgs e )
    {
        try
        {
            string serialNumber = txtSerialNumber.Text.Trim();
            int imageCount = (int)numImageCount.Value;

            if (string.IsNullOrEmpty(serialNumber))
            {
                MessageBox.Show("Serial number cannot be empty!", "Serial Number OCR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
            imageGenerator.GenerateSerialNumberImages(serialNumber, imageCount);
            lblStatus.Text = $"Generated {imageCount} images for serial number {serialNumber}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating images: {ex.Message}", "Generate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ProcessImagesOperation(object sender, EventArgs e )
    {
        try
        {
            if (ocrProcessor == null)
            {
                MessageBox.Show("No OCR processor found!", "No OCR processor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            var results = ocrProcessor.ProcessAllImages(imageGenerator.GetImagesFolder(), dataset);
            dataset.SerialNumbers.AddRange(results);

            UpdateResultsList();
            datasetManager.SaveDataset(dataset);
            
            lblStatus.Text = $"Processed {results.Count} images for serial number {dataset.SerialNumbers.Count}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error processing images: {ex.Message}", "Process Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadDataOperation(object sender, EventArgs e )
    {
        LoadDataset();
    }

    private void SaveDataOperation(object sender, EventArgs e)
    {
        try
        {
            datasetManager.SaveDataset(dataset);
            lblStatus.Text = $"Saved {dataset.SerialNumbers.Count} serial numbers";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving data: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateResultsList()
    {
        lstResults.Items.Clear();
        foreach (var item in dataset.SerialNumbers.OrderByDescending(s => s.SerialNumber))
        {
            lstResults.Items.Add($"{item.SerialNumber} - Confidence: {item.Confidence} - {Path.GetFileName(item.ImagePath)}");
        }
    }

    private void SelectChangingIndex(object sender, EventArgs e)
    {
        if (lstResults.SelectedIndex >= 0)
        {
            var selectedItem = dataset.SerialNumbers.OrderByDescending(s => s.CreatedAt).ToList()[lstResults.SelectedIndex];
            if (File.Exists(selectedItem.ImagePath))
            {
                picPreview.Image?.Dispose();
                picPreview.Image = Image.FromFile(selectedItem.ImagePath);
            }
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        ocrProcessor?.Dispose();
        picPreview.Image?.Dispose();
        base.OnFormClosed(e);
    }
}
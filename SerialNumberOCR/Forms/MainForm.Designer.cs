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
      private Button btnGenerateImage;
      private Button btnProcessImages;
      private Button btnLoadDataset;
      private Button btnSaveDataset;
      private TextBox txtSerialNumber;
      private NumericUpDown numImageCount;
      private ListBox lstResult;
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

      private void InitializeComponent()
      {
            this.Size = new Size(800, 600);
            this.Text = "Serial Number OCR";
            this.StartPosition = FormStartPosition.CenterScreen;
            
            CreateControls();
            SetupEventHandlers();
            AddControlsToForm();
      }

      private void CreateControls()
      {
            btnGenerateImage = new Button {  Text = "Generate Image", Location = new Point(10, 10), Size = new Size(70, 20)};
            btnProcessImages = new Button { Text = "Process Images",  Location = new Point(140, 10), Size = new Size(120, 30)};
            btnLoadDataset = new Button {Text = "Load Dataset", Location = new Point(270, 10), Size = new Size(120, 30) };
            btnSaveDataset = new Button {  Text = "Save Dataset", Location = new Point(400, 10), Size = new Size(120, 30) };
            
            txtSerialNumber = new TextBox {  Location = new Point(10, 70), Size = new Size(200, 25), Text = "SN01234" };
            numImageCount =  new NumericUpDown {Location = new Point(220, 70), Size = new Size(80, 25), Value = 10, Minimum = 1, Maximum = 100};
            
            lstResult = new ListBox{ Location = new Point(10, 110), Size = new Size(400, 300)};
            picPreview = new PictureBox { Location = new Point(420, 110),Size = new Size(400, 300)};
            
            lblStatus = new Label { Location = new Point(10, 420), Size = new Size(600, 25), Text = "Ready" };
      }

      private void SetupEventHandlers()
      {
            btnGenerateImage.Click += BtnGenerateImages_Click;
            btnProcessImages.Click += BtnProcessImages_Click;
            btnLoadDataset.Click += BtnLoadDataset_Click;
            btnSaveDataset.Click += BtnSaveDataset_Click;
            lstResult.SelectedIndexChanged += LstResult_SelectedIndexChanged;
      }

      private void AddControlsToForm()
      {
            this.Controls.AddRange(new Control[]
            {
                  btnGenerateImage, btnProcessImages, btnLoadDataset, btnSaveDataset, txtSerialNumber, numImageCount, lstResult, picPreview, lblStatus
            });
            
            this.Controls.Add(new Label {Text = "serial number", Location = new Point(10, 50), Size = new Size(100, 20)});
            this.Controls.Add(new Label {Text = "Image Count", Location = new Point(220, 50), Size = new Size(100, 20)});
      }

      private void LoadDataset()
      {
            try
            {
                  dataset = datasetManager.LoadDataset();
                  UpdateResultList();
                  lblStatus.Text = $"Dataset loaded with {dataset.SerialNumbers.Count} entries";
            }
            catch (Exception ex)
            {
                  MessageBox.Show($"Error loading dataset: {ex.Message}", "Load Error!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                  dataset = new Dataset();
            }
      }

      private void UpdateResultList()
      {
            lstResult.Items.Clear();
            foreach (var item in dataset.SerialNumbers.OrderByDescending(s => s.CreatedAt))
            {
                  lstResult.Items.Add($"{item.SerialNumber} - Conf: {item.Confidence:F2} - {Path.GetFileName(item.ImagePath)}");
            }
      }

      private void BtnGenerateImages_Click(object sender, EventArgs e)
      {
            try
            {
                  string serialNumber = txtSerialNumber.Text.Trim();
                  int imageCount = (int)numImageCount.Value;

                  if (string.IsNullOrEmpty(serialNumber))
                  {
                        MessageBox.Show("Please enter a serial number", "Input Error", MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
                        return;
                  }

                  imageGenerator.GenerateSerialNumberImages(serialNumber, imageCount);
                  lblStatus.Text = $"Generated {imageCount} images for {serialNumber}";
            }
            catch (Exception ex)
            {
                  MessageBox.Show($"Error generating images: {ex.Message}", "Generate Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
      }

      private void BtnProcessImages_Click(object sender, EventArgs e)
      {
            try
            {
                  if (ocrProcessor == null)
                  {
                        MessageBox.Show("OCR processor not initialized.", "Error!", MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                        return;
                  }

                  var results =
                        ocrProcessor.ProcessAllImages(imageGenerator.GetImageFolder(),
                              dataset); //GetImageFolder() error because the class not public in ImageGenerator.cs
                  dataset.SerialNumbers.AddRange(results);

                  UpdateResultList();
                  datasetManager.SaveDataset(dataset);

                  lblStatus.Text = $"Processed {results.Count} new images. Total: {dataset.SerialNumbers.Count}";
            }
            catch (Exception ex)
            {
                  MessageBox.Show($"Error processing images: {ex.Message}", "Process Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
      }

      private void BtnLoadDataset_Click(object sender, EventArgs e)
      {
            LoadDataset();
      }

      private void BtnSaveDataset_Click(object sender, EventArgs e)
      {
            try
            {
                  datasetManager.SaveDataset(dataset);
                  lblStatus.Text = $"Dataset saved with {dataset.SerialNumbers.Count} entries";
            }
            catch (Exception ex)
            {
                  MessageBox.Show($"Error saving dataset: {ex.Message}", "Save Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
      }

      private void LstResult_SelectedIndexChanged(object sender, EventArgs e)
      {
            if (lstResult.SelectedIndex >= 0)
            {
                  var selectedItem = dataset.SerialNumbers.OrderByDescending(s => s.CreatedAt).ToList()[lstResult.SelectedIndex];
                  if (File.Exists(selectedItem.ImagePath))
                  {
                        picPreview.Image?.Dispose();
                        picPreview.Image = Image.FromFile(selectedItem.ImagePath);
                  }
            }
      }

      protected override void OnFormClosing(FormClosingEventArgs e)
      {
            ocrProcessor?.Dispose();
            picPreview.Image?.Dispose();
            base.OnFormClosing(e);
      }
}
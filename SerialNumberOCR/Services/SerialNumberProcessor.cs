using System;
using SerialNumberOCR.Models;
using SerialNumberOCR.Services;

namespace SerialNumberOCR.Services;
public class SerialNumberProcessor : IDisposable
{
    private readonly OCRProcessor ocrProcessor;
    private readonly DatasetManager datasetManager;
    private readonly ImageGenerator imageGenerator;
    private Dataset dataset;

    public SerialNumberProcessor(string tessDataPath = @"./tessdata", string datasetPath = "dataset.json")
    {
        ocrProcessor = new OCRProcessor(tessDataPath);
        datasetManager = new DatasetManager(datasetPath);
        imageGenerator = new ImageGenerator();
        dataset = datasetManager.LoadDataset();
    }

    public void GenerateTrainingData(string serialNumber, int imageCount)
    {
        imageGenerator.GenerateSerialNumberImages(serialNumber, imageCount);
    }

    public void ProcessImage(string imagePath)
    {
        var result = ocrProcessor.ProcessImage(imagePath);
        if (string.IsNullOrEmpty(result.SerialNumber))
        {
            dataset.SerialNumbers.Add(result);
        }
    }

    public void ProcessAllGeneratedImages()
    {
        var processImages = ocrProcessor.ProcessAllImages(imageGenerator.GetImagesFolder(), dataset);
        dataset.SerialNumbers.AddRange(processImages);
    }

    public void SaveDataset()
    {
        datasetManager.SaveDataset(dataset);
    }
    
    public Dataset GetDataset() => dataset;
    public void Dispose()
    {
        ocrProcessor?.Dispose();
    }
}
using System;
using System.IO;
using System.Text.Json;
using SerialNumberOCR.Models;
using SerialNumberOCR.Services;


namespace SerialNumberOCR.Services
{
    public class DatasetManager
    {
        private readonly string datasetPath;
        private readonly JsonSerializerOptions jsonOptions;

        public DatasetManager(string filePath = "dataset.json")
        {
                datasetPath = filePath;
                jsonOptions = new JsonSerializerOptions {WriteIndented = true};
        }

        public Dataset LoadDataset()
        {
            try
            {
                if (File.Exists(datasetPath))
                {
                        string json = File.ReadAllText(datasetPath);
                        return JsonSerializer.Deserialize<Dataset>(json) ?? new Dataset();
                }
                return new Dataset();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error reading dataset from{datasetPath}: {ex.Message}", ex);;
            }
        }

        public void SaveDataset(Dataset dataset)
        {
            try
            {
                dataset.LastUpdated = DateTime.Now;
                dataset.TotalImages = dataset.SerialNumbers.Count;

                string json = JsonSerializer.Serialize(dataset, jsonOptions);
                File.WriteAllText(datasetPath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error saving dataset to {datasetPath}: {ex.Message}", ex);
            }
        }

        public void ExportDataset(Dataset dataset, string exportPath)
        {
            try
            {
                string json = JsonSerializer.Serialize(dataset, jsonOptions);
                File.WriteAllText(exportPath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error exporting dataset to {exportPath}: {ex.Message}", ex);
            }
        }

        public Dataset ImportDataset(string importPath)
        {
            try
            {
                if (!File.Exists(importPath))
                    throw new FileNotFoundException($"File {importPath} does not exist");

                string json = File.ReadAllText(importPath);
                return JsonSerializer.Deserialize<Dataset>(json) ?? new Dataset();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error importing dataset from {importPath}: {ex.Message}", ex);
            }
        }
    }
}
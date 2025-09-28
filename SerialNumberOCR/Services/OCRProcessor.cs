using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tesseract;
using SerialNumberOCR.Models;

namespace SerialNumberOCR.Services
{
    public class OCRProcessor : IDisposable
    {
        private TesseractEngine tesseractEngine;
        private bool disposed = false;

        public OCRProcessor(string tessDataPath = @"./tessdata", string language = "eng")
        {
            InitializeTesseract(tessDataPath, language);
        }

        private void InitializeTesseract(string tessDataPath, string language)
        {
            try
            {
                tesseractEngine = new TesseractEngine(tessDataPath, language, EngineMode.Default);
                tesseractEngine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Could not initialize tesseract engine", ex);
            }
        }

        public SerialNumberData ProcessImage(string imagePath)
        {
            if (tesseractEngine == null)
                throw new InvalidOperationException("No tesseract engine is initialized");

            try
            {
                using (var image = Pix.LoadFromFile(imagePath)
                {
                    using (var page = tesseractEngine.Process(image))
                    {
                        string text = page.GetText().Trim();
                        float confidence = page.GetMeanConfidence();
                        return new SerialNumberData()
                        {
                            SerialNumber = text,
                            ImagePath = imagePath,
                            CreatedAt = DateTime.Now,
                            Confidence = confidence,
                            BoundingBox = new BoundingBox
                            {
                                X = 0,
                                Y = 0,
                                Width = image.Width,
                                Height = image.Height
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not process image, {imagePath}: {ex.Message}", ex);
            }
        }

        public List<SerialNumberData> ProcessAllImages(string imageFolder, Dataset existingDataset = null,
            double minimumConfidence = 0.5)
        {
            var result = new List<SerialNumberData>();
            string[] files = Directory.GetFiles(imageFolder, "*.png");

            var processedPath = existingDataset?.SerialNumbers?.Select(s => s.ImagePath).ToHashSet() ??
                                new HashSet<string>();

            foreach (var imagePath in imageFolder)
            {
                if (processedPath.Contains(imagePath))
                    continue;
                try
                {
                    var result = ProcessImage(imagePath);

                    if (!string.IsNullOrEmpty(result.SerialNumber) && resultConfidence >= minimumConfidence)
                    {
                        result.Add(result);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {imagePath}: {ex.Message}");
                }
            }

            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    tesseractEngine?.Dispose();
                }

                disposed = true;
            }
        }
    }
}
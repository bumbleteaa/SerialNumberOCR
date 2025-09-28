using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SerialNumberOCR;

namespace SerialNumberOCR.Services;

public class ImageGenerator
{
    private readonly Random random;
    private readonly string imagesFolder;

    public ImageGenerator(string outPutFolder = "generate images")
    {
        random = new Random();
        imagesFolder = outPutFolder;
        CreateImagesFolder();
    }

    private void CreateImagesFolder()
    {
        if (!Directory.Exists(imagesFolder))
            Directory.CreateDirectory(imagesFolder);
    }

    public void GenerateSerialNumberImages(string serialNumber, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Bitmap image = CreateSerialNumberImages(serialNumber, i);
            string fileName = $"{serialNumber}_{DateTime.Now:yyyyMMdd_HHmmss}_{i:000}.png";
            string filePath = Path.Combine(imagesFolder, fileName);

            image.Save(filePath, ImageFormat.Png);
            image.Dispose();
        }
    }

    private Bitmap CreateSerialNumberImages(string serialNumber, int variation)
    {
        int width = 400 + random.Next(-50, 50);
        int height = 100 + random.Next(-20, 20);
        Bitmap bitmap = new Bitmap(width, height);

        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            ApplyBackground(graphics, width, height);
            DrawSerialNumber(graphics, serialNumber, width, height);

            if (random.Next(3) == 0)
            {
                AddNoise(graphics, width, height);
            }
        }

        return bitmap;
    }

    private void ApplyBackground(Graphics graphics, int width, int height)
    {
        Color[] backgroundColors = { Color.White, Color.LightGray, Color.WhiteSmoke, Color.AliceBlue, };
        graphics.Clear(backgroundColors[random.Next(backgroundColors.Length)]);
    }

    private void DrawSerialNumber(Graphics graphics, string serialNumber, int width, int height)
    {
        string[] fontNames = { "Arial", "Times New Roman" };
        float fontSize = 18 + random.Next(-4, 8);
        FontStyle fontStyle = random.Next(3) == 0 ? FontStyle.Bold : FontStyle.Regular;

        using (Font font = new Font(fontNames[random.Next(fontNames.Length)], fontSize, fontStyle))
        {
            Color[] textColors = { Color.Black, Color.DarkBlue, Color.DarkGreen, Color.DarkRed };
            using (Brush brush = new SolidBrush(textColors[random.Next(textColors.Length)]))
            {
                float x = 10 + random.Next(20);
                float y = height / 2 - fontSize / 2 + random.Next(-10, 10);

                graphics.DrawString(serialNumber, font, brush, x, y);
            }
        }
    }

    private void AddNoise(Graphics graphics, int width, int height)
    {
        using (Pen pen = new Pen(Color.LightGray, 1))
        {
            for (int i = 0; i < 20; i++)
            {
                graphics.DrawLine(pen, random.Next(width), random.Next(height), random.Next(width),
                    random.Next(height));
            }
        }

        string GetImagesFolder() => imagesFolder; //Must be public because it will mentioned in MainForm.cs 
    }
}
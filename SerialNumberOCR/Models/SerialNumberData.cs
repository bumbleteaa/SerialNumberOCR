using System;
namespace SerialNumberOCR.Models;

public class SerialNumberData
{
    public string SerialNumber { get; set; }
    public string ImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public BoundingBox BoundingBox { get; set; }
    public double Confidence { get; set; }
}

public class BoundingBox
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class Dataset 
{
    public List<SerialNumberData> SerialNumbers { get; set; } = new List<SerialNumberData>();
    public DateTime LastUpdated { get; set; }
    public int TotalImages { get; set; }
}
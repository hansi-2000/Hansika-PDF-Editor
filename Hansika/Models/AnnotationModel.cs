using System;

namespace Hansika.Models
{
    public class Annotation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = ""; // "sticky-note", "free-text", "callout"
        public string Text { get; set; } = "";
        public int PageNumber { get; set; } = 1;
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; } = 100;
        public float Height { get; set; } = 30;
        public string Color { get; set; } = "#FFFF00";
        public int FontSize { get; set; } = 12;
        public string TextColor { get; set; } = "#000000";
        public string BackgroundColor { get; set; } = "#FFFFFF";
        public string BorderColor { get; set; } = "#000000";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Subject { get; set; } = "";
    }

    public class AnnotationList
    {
        public static List<Annotation> Annotations { get; set; } = new List<Annotation>();
    }
}

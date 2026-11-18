using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace TODO
{
    internal class TODO
    {
        public int Version { get; set; } = 1;

        public List<DayItem> Days { get; set; } = new List<DayItem>();

        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        public AppSettings settings { get; set; } = new AppSettings();
    }

    public class DayItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string Title { get; set; } = "";
    }

    public class TaskItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string Name { get; set; } = "";

        public Dictionary<string, bool> CompletedDays { get; set; } = new Dictionary<string, bool>();
    }

    public class AppSettings
    {
        public string BackgroundColor { get; set; } = "Sysyem";

        public string TextColor { get; set; } = "System";

        public string FontName { get; set; } = "";

        public float FontSize { get; set; } = 9.0f;
    }
}

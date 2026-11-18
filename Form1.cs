using System;
using System.IO;
using System.Drawing;
using System.Text.Json;
using System.Configuration;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime;


namespace TODO
{
    public partial class Form1 : Form
    {
        
        private int selectedTaskRow = -1;

        private ContextMenuStrip taskOptionsMenu = new ContextMenuStrip();

        private TODO todoData = new TODO();
        
        private bool isLoadingData = false;

        private readonly string saveFilePatch =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData), "TODO", "todo-data.json");
        public Form1()
        {
            InitializeComponent();

            ConfigureGrid();

            LoadData();

            BuildGridFromData();

            btnAddDay.Click += BtnAddDay_Click;
            btnAddTask.Click += btnAddTask_Click;

            dataGridView1.CellContentClick += DataGridView1_CellContentClick;

            dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;

            dataGridView1.CellValueChanged += DataGridView1_CellEndEdit;

            dataGridView1.CurrentCellDirtyStateChanged += DataGridView1_CurrentCellDirtyStateChanged;

            FormClosing += Form1_FormClosing;

            taskOptionsMenu.Items.Add("Delete Task", null, DeleteTask_Click);
        }

        public void ConfigureGrid()
        {
            dataGridView1.Columns.Clear();

            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            DataGridViewTextBoxColumn taskColumn =
                new DataGridViewTextBoxColumn();

            taskColumn.Name = "TaskColumn";

            taskColumn.HeaderText = "Task";

            taskColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            taskColumn.MinimumWidth = 180;

            dataGridView1.Columns.Add(taskColumn);

            

            DataGridViewButtonColumn optionsColumn = new DataGridViewButtonColumn();

            optionsColumn.Name = "OptionsColumn";
            optionsColumn.HeaderText = "";
            optionsColumn.Text = "...";
            optionsColumn.UseColumnTextForButtonValue = true;
            optionsColumn.Width = 45;
            optionsColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

            
            // Options column.
            DataGridViewButtonColumn OptionsColumn =
                new DataGridViewButtonColumn();

            optionsColumn.Name = "OptionsColumn";
            optionsColumn.HeaderText = "";
            optionsColumn.Text = "...";
            optionsColumn.UseColumnTextForButtonValue = true;
            optionsColumn.Width = 45;
            optionsColumn.SortMode =
                DataGridViewColumnSortMode.NotSortable;

            dataGridView1.Columns.Add(optionsColumn);
        }

        private void LoadData()
        {
            if(File.Exists(saveFilePatch))
            {
                try
                {
                    string json = File.ReadAllText(saveFilePatch);

                    TODO? LoadedData = JsonSerializer.Deserialize<TODO>(json);

                    if (LoadedData != null)
                    {
                        todoData = LoadedData;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Could not load saved data:\n" + ex.Message,
                        "Loading Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    todoData = new TODO();
                }
            }

            todoData.Days ??= new List<DayItem>();
            todoData.Tasks ??= new List<TaskItem>();
            todoData.settings ??= new AppSettings();

            foreach (TaskItem task in todoData.Tasks)
            {
                task.CompletedDays??= new Dictionary<string, bool>();
            }

            if (todoData.Days.Count == 0)
            {
                todoData.Days.Add(
                    new DayItem
                    {
                        Title = DateTime.Today.ToString("M/d")
                    }
                );
            }
        }

        private void BuildGridFromData()
        {
            isLoadingData = true;

            try
            {
                dataGridView1.Rows.Clear();

                foreach(DayItem day in todoData.Days)
                {
                    AddDayColumn(day);
                }
                foreach(TaskItem task in todoData.Tasks)
                {
                    int rowIndex = dataGridView1.Rows.Add();

                    DataGridViewRow row = dataGridView1.Rows[rowIndex];

                    row.Tag = task.Id;

                    row.Cells["TaskColumn"].Value = task.Name;

                    foreach(DayItem day in todoData.Days)
                    {
                        row.Cells[day.Id].Value =
                            task.CompletedDays.TryGetValue(day.Id, out bool completed) ? completed : false;
                    }
                }
            }
            finally
            {
                isLoadingData = false;
            }
        }

        private void AddDayColumn(DayItem day)
        {
            

            DataGridViewCheckBoxColumn dayColumn = new DataGridViewCheckBoxColumn();

            dayColumn.Name = day.Id;

            dayColumn.HeaderText = day.Title;

            dayColumn.Width = 80;

            dayColumn.TrueValue = true;
            dayColumn.FalseValue = false;

            int optionsIndex = dataGridView1.Columns["OptionsColumn"].Index;

            dataGridView1.Columns.Insert(optionsIndex, dayColumn);
        }

        private void btnAddTask_Click(object? sender, EventArgs e)
        {

            isLoadingData = true;

            int rowIndex;
            
            try
            {
                rowIndex = dataGridView1.Rows.Add();

                DataGridViewRow newRow = dataGridView1.Rows[rowIndex];

                newRow.Tag = Guid.NewGuid().ToString("N");

                newRow.Cells["TaskColumn"].Value = "New Task";

                foreach (DayItem day in todoData.Days)
                {
                    newRow.Cells[day.Id].Value = false;
                }
            }
            finally
            {
                isLoadingData = false;
            }

            SaveData();

            dataGridView1.CurrentCell = dataGridView1.Rows[rowIndex].Cells["TaskColumn"];

            dataGridView1.BeginEdit(true);
        }

        private void SaveData()
        {
            if (isLoadingData)
                return;

            try
            {
                List<TaskItem> updatedTasks =
                    new List<TaskItem>();

                // Read every task currently displayed.
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow)
                        continue;

                    string taskId =
                        row.Tag as string ??
                        Guid.NewGuid().ToString("N");

                    // Find the existing task, if one exists.
                    TaskItem? task =
                        todoData.Tasks.Find(t => t.Id == taskId);

                    // Otherwise create a new task.
                    if (task == null)
                    {
                        task = new TaskItem
                        {
                            Id = taskId
                        };
                    }

                    task.Name =
                        Convert.ToString(
                            row.Cells["TaskColumn"].Value
                        ) ?? "";

                    // Rebuild the checkbox information.
                    task.CompletedDays =
                        new Dictionary<string, bool>();

                    foreach (DayItem day in todoData.Days)
                    {
                        object? cellValue =
                            row.Cells[day.Id].Value;

                        bool completed =
                            cellValue is bool value && value;

                        task.CompletedDays[day.Id] = completed;
                    }

                    // Preserve the order in which rows appear.
                    updatedTasks.Add(task);
                }

                // Update the saved task list.
                todoData.Tasks = updatedTasks;

                // Make sure the save directory exists.
                string? directory =
                    Path.GetDirectoryName(saveFilePatch);

                if (directory != null)
                {
                    Directory.CreateDirectory(directory);
                }

                // Convert our data to JSON.
                string json = JsonSerializer.Serialize(
                    todoData,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );

                // Write the JSON file.
                File.WriteAllText(saveFilePatch, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not save data:\n" + ex.Message,
                    "Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void DataGridView1_CellValueChanged(
    object? sender, DataGridViewCellEventArgs e)
        {
            if (isLoadingData)
                return;

            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            // Only save checkbox changes here.
            if (dataGridView1.Columns[e.ColumnIndex]
                is DataGridViewCheckBoxColumn)
            {
                SaveData();
            }
        }


        private void DataGridView1_CellEndEdit(
    object? sender, DataGridViewCellEventArgs e)
        {
            if (isLoadingData)
                return;

            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (dataGridView1.Columns[e.ColumnIndex].Name
                == "TaskColumn")
            {
                SaveData();
            }
        }

        private void DataGridView1_CurrentCellDirtyStateChanged(
    object? sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty &&
                dataGridView1.CurrentCell is DataGridViewCheckBoxCell)
            {
                dataGridView1.CommitEdit(
                    DataGridViewDataErrorContexts.Commit
                );
            }
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (dataGridView1.Columns[e.ColumnIndex].Name != "OptionsColumn")
                return;

            selectedTaskRow = e.RowIndex;

            Rectangle cellRectangle = dataGridView1.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);

            taskOptionsMenu.Show(dataGridView1, new Point(cellRectangle.Left, cellRectangle.Bottom));
        }

        private void DeleteTask_Click(object? sender, EventArgs e)
        {
            if (selectedTaskRow >= 0 &&
                selectedTaskRow < dataGridView1.Rows.Count)
            {
                dataGridView1.Rows.RemoveAt(selectedTaskRow);

                selectedTaskRow = -1;

                SaveData();
            }
        }

        private void BtnAddDay_Click(object? sender, EventArgs e)
        {
            string title = AskForColumnTitle(DateTime.Today.ToString("M/d"));

            if (string.IsNullOrWhiteSpace(title))
                return;

            DayItem newDay = new DayItem
            {
                Title = title,
            };

            todoData.Days.Add(newDay);

            isLoadingData = true;

            try
            {
                AddDayColumn(newDay);

                foreach(DataGridViewRow row in dataGridView1.Rows)
                {
                    row.Cells[newDay.Id].Value = false;
                }
            }
            finally
            {
                isLoadingData = false;
            }

            SaveData();
        }

        private string AskForColumnTitle(string defaultText)
        {
            Form prompt = new Form();

            prompt.Text = "Add a new day";
            prompt.Width = 300;
            prompt.Height = 140;
            prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
            prompt.StartPosition = FormStartPosition.CenterScreen;
            prompt.MinimizeBox = false;
            prompt.MaximizeBox = false;

            TextBox input = new TextBox();
            input.Left = 12;
            input.Top = 12;
            input.Width = 260;
            input.Text = defaultText;

            Button addButton = new Button();
            addButton.Text = "Add";
            addButton.Left = 112;
            addButton.Top = 48;
            addButton.Width = 75;
            addButton.DialogResult = DialogResult.OK;

            Button cancelbutton = new Button();
            cancelbutton.Text = "Cancel";
            cancelbutton.Left = 197;
            cancelbutton.Top = 48;
            cancelbutton.Width = 75;
            cancelbutton.DialogResult = DialogResult.Cancel;

            prompt.Controls.Add(input);
            prompt.Controls.Add(addButton);
            prompt.Controls.Add(cancelbutton);

            prompt.AcceptButton = addButton;
            prompt.CancelButton = cancelbutton;

            string result = "";



            if (prompt.ShowDialog(this) == DialogResult.OK)
            {
                result = input.Text.Trim();
            }

            prompt.Dispose();

            return result;
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(
    object? sender, FormClosingEventArgs e)
        {
            dataGridView1.EndEdit();

            SaveData();
        }

    }
}

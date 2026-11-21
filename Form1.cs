using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Runtime;
using System.Text.Json;
using System.Configuration;
using System.Windows.Forms;
using System.Collections.Generic;



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

            btnAddDay.Visible = false;
            btnAddDay.Enabled = false;

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

           
        }

        private void BuildGridFromData()
        {
            isLoadingData = true;

            try
            {
                dataGridView1.Rows.Clear();
                for (int i = dataGridView1.Columns.Count -1; i >= 0; i--)
                {
                    if (dataGridView1.Columns[i].Name.StartsWith("Day_"))
                    {
                        dataGridView1.Columns.RemoveAt(i);
                    }
                }

                DateTime today = DateTime.Today;
                DateTime firstday = today.AddDays(-6);

                for (DateTime date = firstday; date <= today; date = date.AddDays(1))
                {
                    AddDayColumn(date);
                }

                foreach (TaskItem task in todoData.Tasks)
                {
                    int rowindex = dataGridView1.Rows.Add();

                    DataGridViewRow row = dataGridView1.Rows[rowindex];

                    row.Tag = task.Id;
                    row.Cells["TaskColumn"].Value = task.Name;

                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        if (column is DataGridViewCheckBoxColumn)
                        {
                            string datakey = column.Name.Substring("Day_".Length);

                            bool completed = false;

                            if(task.CompletedDays != null)
                            {
                                task.CompletedDays.TryGetValue(datakey, out completed);
                            }

                            row.Cells[column.Index].Value = completed;
                        }
                    }
                }
            }
            finally
            {
                isLoadingData = false;
            }
        }

        private void AddDayColumn(DateTime date)
        {
            string datekey  = date.ToString("yyyy-MM-dd");

            DataGridViewCheckBoxColumn daycolumn = new DataGridViewCheckBoxColumn();

            daycolumn.Name = "Day_" + datekey;
            daycolumn.HeaderText = date.ToString("M/d");
            daycolumn.Width = 80;

            daycolumn.ReadOnly = date.Date != DateTime.Today;

            daycolumn.TrueValue = true;
            daycolumn.FalseValue = false;

            int optionsIndex = dataGridView1.Columns["OptionsColumn"].Index;

            dataGridView1.Columns.Insert(optionsIndex, daycolumn);
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

                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    if (column is DataGridViewCheckBoxColumn)
                    {
                        newRow.Cells[column.Index].Value = false;
                    }
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
                List<TaskItem> updatedTasks = new List<TaskItem>();

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow)
                        continue;

                    string taskId = row.Tag as string
                        ?? Guid.NewGuid().ToString("N");

                    row.Tag = taskId;

                    TaskItem? task =
                        todoData.Tasks.FirstOrDefault(t => t.Id == taskId);

                    if (task == null)
                    {
                        task = new TaskItem
                        {
                            Id = taskId
                        };
                    }

                    task.Name =
                        Convert.ToString(row.Cells["TaskColumn"].Value) ?? "";

                    task.CompletedDays ??= new Dictionary<string, bool>();

                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        if (column is DataGridViewCheckBoxColumn)
                        {
                            string dateKey =
                                column.Name.Substring("Day_".Length);

                            object? cellValue = row.Cells[column.Index].Value;

                            bool completed = cellValue != null &&
                                             Convert.ToBoolean(cellValue);

                            // Update this date without deleting older records.
                            task.CompletedDays[dateKey] = completed;
                        }
                    }

                    updatedTasks.Add(task);
                }

                todoData.Tasks = updatedTasks;

                string? directory = Path.GetDirectoryName(saveFilePatch);

                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(
                    todoData,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                File.WriteAllText(saveFilePatch, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not save the data:\n" + ex.Message,
                    "Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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

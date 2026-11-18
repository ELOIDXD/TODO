using System;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;

namespace TODO
{
    public partial class Form1 : Form
    {
        private int dayColumnNumber = 0;
        private int selectedTaskRow = -1;

        private ContextMenuStrip taskOptionsMenu = new ContextMenuStrip();
        public Form1()
        {
            InitializeComponent();

            ConfigureGrid();

            btnAddDay.Click += BtnAddDay_Click;
            btnAddTask.Click += btnAddTask_Click;

            dataGridView1.CellContentClick += DataGridView1_CellContentClick;

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

            dataGridView1.Columns.Add(optionsColumn);
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

           

            // Add today's date after the options column exists.
            AddDayColumn(DateTime.Today.ToString("M/d"));
        }

        private void AddDayColumn(string title)
        {
            dayColumnNumber++;

            DataGridViewCheckBoxColumn dayColumn = new DataGridViewCheckBoxColumn();

            dayColumn.Name = "DayColumn" + dayColumnNumber;

            dayColumn.HeaderText = title;

            dayColumn.Width = 80;

            int optionsIndex = dataGridView1.Columns["OptionsColumn"].Index;

            dataGridView1.Columns.Insert(optionsIndex, dayColumn);
        }

        private void btnAddTask_Click(object? sender, EventArgs e)
        {
            int rowIndex = dataGridView1.Rows.Add();

            DataGridViewRow newRow =
                dataGridView1.Rows[rowIndex];

            // Set default task name.
            newRow.Cells["TaskColumn"].Value = "New Task";

            // Initialize checkbox columns.
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                if (column is DataGridViewCheckBoxColumn)
                {
                    newRow.Cells[column.Index].Value = false;
                }
            }

            // Select the task name.
            dataGridView1.CurrentCell =
                newRow.Cells["TaskColumn"];

            // Start editing.
            dataGridView1.BeginEdit(true);
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
            if (selectedTaskRow >= 0 && selectedTaskRow < dataGridView1.Rows.Count)
            {
                dataGridView1.Rows.RemoveAt(selectedTaskRow);

                selectedTaskRow = -1;
            }
        }

        private void BtnAddDay_Click(object? sender, EventArgs e)
        {
            string title = AskForColumnTitle(DateTime.Today.ToString("M/d"));

            if (string.IsNullOrWhiteSpace(title))
                return;

            AddDayColumn(title);
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

        
    }
}

using System.Configuration;

namespace TODO
{
    public partial class Form1 : Form
    {
        private int dayColumnNumber = 0;
        public Form1()
        {
            InitializeComponent();

            ConfigureGrid();

            btnAddDay.Click += BtnAddDay_Click;
        }

        public void ConfigureGrid()
        {
            dataGridView1.Columns.Clear();

            dataGridView1.AllowUserToAddRows = true;
            dataGridView1.AllowUserToDeleteRows = true;

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            DataGridViewTextBoxColumn taskColumn = 
                new DataGridViewTextBoxColumn();

            taskColumn.Name = "TaskColumn";

            taskColumn.HeaderText = "Task";

            taskColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            taskColumn.MinimumWidth = 180;

            dataGridView1.Columns.Add(taskColumn);

            AddDayColumn(DateTime.Today.ToString("M/d"));
        }

        private void AddDayColumn(string title)
        {
            dayColumnNumber++;

            DataGridViewCheckBoxColumn dayColumn = new DataGridViewCheckBoxColumn();

            dayColumn.Name = "DayColumn" + dayColumnNumber;

            dayColumn.HeaderText = title;

            dayColumn.Width = 80;

            dataGridView1.Columns.Add(dayColumn);
        }

        private void BtnAddDay_Click(object sender, EventArgs e)
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

            if(prompt.ShowDialog(this) == DialogResult.OK)
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

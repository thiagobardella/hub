using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ExcelDataReader;
using ExcelDataReader.Exceptions;

namespace ClientsRegistration
{
    public partial class FormMain : Form
    {
        Client currentClient = new Client();

        public FormMain()
        {
            if (!CheckDatabaseExists())
                GenerateDatabase();
            InitializeComponent();
            dtpCreatedAt.Value = DateTime.Now;
        }

        private bool CheckDatabaseExists()
        {
            SqlConnection sqlConnection = new SqlConnection(@"data source=.\SQLEXPRESS;initial catalog=Hub;integrated security=SSPI;MultipleActiveResultSets=True;App=EntityFramework");
            try
            {
                sqlConnection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void GenerateDatabase()
        {
            List<string> cmds = new List<string>();
            if (File.Exists(Application.StartupPath + "\\ScriptDatabase.sql"))
            {
                TextReader textReader = new StreamReader(Application.StartupPath + "\\ScriptDatabase.sql");
                string line = "";
                string cmd = "";
                while((line = textReader.ReadLine()) != null)
                {
                    if (line.Trim().ToUpper() == "GO")
                    {
                        cmds.Add(cmd);
                        cmd = "";
                    }
                    else
                    {
                        cmd += line + "\r\n";
                    }
                }
                if (cmd.Length > 0)
                {
                    cmds.Add(cmd);
                    cmd = "";
;               }
                textReader.Close();
            }
            if (cmds.Count > 0)
            {
                SqlCommand sqlCommand = new SqlCommand
                {
                    Connection = new SqlConnection(@"Server=.\SQLEXPRESS;Integrated security=SSPI;database=master"),
                    CommandType = CommandType.Text
                };
                sqlCommand.Connection.Open();
                foreach(var cmd in cmds)
                {
                    sqlCommand.CommandText = cmd;
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        void Clear()
        {
            txtName.Text = txtEmail.Text = "";
            dtpCreatedAt.Value = DateTime.Now;
            btnAdd.Text = "Add";
            btnDelete.Enabled = false;
            currentClient.Id = 0;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            PopulateDataGridView();
            dtpCreatedAt.Format = dtpFromDate.Format = dtpToDate.Format = DateTimePickerFormat.Custom;
            dtpCreatedAt.CustomFormat = dtpFromDate.CustomFormat = dtpToDate.CustomFormat = "dd/MM/yyyy";
        }

        private void PopulateDataGridView()
        {
            using (HubEntities db = new HubEntities())
            {
                db.Clients.Load();
                clientsBindingSource.DataSource = db.Clients.Local;
            }
            Clear();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this record?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                using (HubEntities db = new HubEntities())
                {
                    var entry = db.Entry(currentClient);
                    if (entry.State == EntityState.Detached)
                        db.Clients.Attach(currentClient);
                    db.Clients.Remove(currentClient);
                    db.SaveChanges();
                    PopulateDataGridView();
                    Clear();
                    MessageBox.Show("Client deleted successfully!");
                }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (txtName.Text == String.Empty)
            {
                MessageBox.Show("Name is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtEmail.Text == String.Empty)
            {
                MessageBox.Show("Email is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            currentClient.Name = txtName.Text.Trim();
            currentClient.Email = txtEmail.Text.Trim();
            currentClient.CreatedAt = dtpCreatedAt.Value;

            using (HubEntities db = new HubEntities())
            {
                if (db.Clients.AsEnumerable().Contains(currentClient))
                {
                    Client existentClient = db.Clients.Where(x => x.Name == currentClient.Name && x.Email == currentClient.Email).FirstOrDefault();
                    MessageBox.Show("Client already registered on date " + existentClient.CreatedAt.ToString("dd/MM/yyyy") + " !", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (currentClient.Id == 0)
                    db.Clients.Add(currentClient);
                else
                    db.Entry(currentClient).State = EntityState.Modified;
                db.SaveChanges();
            }

            PopulateDataGridView();
            Clear();
            MessageBox.Show("Client saved successfully!");
        }

        private void DgvClients_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvClients == null)
                return;

            if (dgvClients.CurrentRow.Index != -1)
            {
                currentClient.Id = Convert.ToInt32(dgvClients.CurrentRow.Cells[0].Value);
                using (HubEntities db = new HubEntities())
                {
                    currentClient = db.Clients.Where(x => x.Id == currentClient.Id).FirstOrDefault();
                    txtName.Text = currentClient.Name;
                    txtEmail.Text = currentClient.Email;
                    dtpCreatedAt.Value = currentClient.CreatedAt;
                }
                btnAdd.Text = "Update";
                btnDelete.Enabled = true;
            }
        }

        private void DgvClients_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            dgv.ClearSelection();
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            using (HubEntities db = new HubEntities())
            {
                List<Client> clientsFiltered = db.Clients.Where(
                    x =>    x.CreatedAt >= dtpFromDate.Value && 
                            x.CreatedAt <= dtpToDate.Value &&
                            (x.Email.ToLower().Contains(txtSearch.Text.ToLower()) || x.Name.ToLower().Contains(txtSearch.Text.ToLower()) || txtSearch.Text.Trim() == String.Empty))
                    .ToList<Client>();
                clientsBindingSource.DataSource = clientsFiltered;
            }
        }

        private void BtnUpload_Click(object sender, EventArgs e)
        {
            if (ofdUpload.ShowDialog().Equals(DialogResult.OK))
            {
                try
                {
                    using (var stream = File.Open(ofdUpload.FileName, FileMode.Open, FileAccess.Read))
                    {

                        IExcelDataReader reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream);

                        var conf = new ExcelDataSetConfiguration
                        {
                            ConfigureDataTable = _ => new ExcelDataTableConfiguration
                            {
                                UseHeaderRow = true
                            }
                        };

                        var dataTable = reader.AsDataSet(conf).Tables[0];

                        using (HubEntities db = new HubEntities())
                        {
                            List<Client> clientList = (from DataRow dr in dataTable.Rows
                                                       select new Client()
                                                       {
                                                           Name = dr["Name"].ToString(),
                                                           Email = dr["Email"].ToString(),
                                                           CreatedAt = DateTime.Parse(dr["Date"].ToString())
                                                       })
                                          .Where(x => !db.Clients.AsEnumerable().Contains(x) &&
                                                       x.Name.Trim() != "" &&
                                                       x.Email.Trim() != "")
                                          .Distinct()
                                          .ToList();

                            if (clientList.Count == 0)
                            {
                                MessageBox.Show("File has no new valid clients!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            db.Clients.AddRange(clientList);
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex is HeaderException || ex is ArgumentException)
                    {
                        MessageBox.Show("File must be an Excel file with Headers: Name, Email and Date", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (ex is FormatException)
                    {
                        MessageBox.Show("'Date' must be on format dd/MM/yyyy", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    MessageBox.Show("Import file failed! Error Message: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                PopulateDataGridView();
                Clear();
                MessageBox.Show("Clients uploaded successfully!");
            }
        }
    }
}

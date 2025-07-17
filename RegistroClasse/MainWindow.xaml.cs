using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;

namespace RegistroDiClasse
{
    public partial class MainWindow : Window
    {
        string connectionStr = "Server=WIN-BG5QTDS27CD\\SQLEXPRESS;Database=RegistroDiClasse;Trusted_Connection=True;TrustServerCertificate=True;";
        public MainWindow()
        {
            InitializeComponent();
            CaricaClassi();
        }

        private void CaricaClassi()
        {
            using SqlConnection connection = new SqlConnection(connectionStr);
            connection.Open();

            SqlCommand command = new SqlCommand("SELECT * FROM Classi", connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            System.Data.DataTable dataTable = new System.Data.DataTable();
            adapter.Fill(dataTable);

            DataRow newRow = dataTable.NewRow();
            newRow["IdClasse"] = 0;
            newRow["NomeClasse"] = "--SELEZIONA UNA CLASSE--";
            dataTable.Rows.InsertAt(newRow, 0);

            comboBoxClassi.ItemsSource = dataTable.DefaultView;
            comboBoxClassi.DisplayMemberPath = "NomeClasse";
            comboBoxClassi.SelectedValuePath = "IdClasse";
            comboBoxClassi.SelectedIndex = 0;
        }

        private void BtnCreaClasse(object sender, RoutedEventArgs e)
        {
            using SqlConnection connection = new SqlConnection(connectionStr);
            SqlCommand command = new SqlCommand("CreaClasse", connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@NomeClasse", txtNomeClasse.Text);
            command.Parameters.AddWithValue("@Anno", int.Parse(txtAnno.Text));
            connection.Open();
            try
            {
                command.ExecuteNonQuery();
                MessageBox.Show("Classe creata con successo!");
                txtNomeClasse.Text = "";
                txtAnno.Text = "";
                CaricaClassi();
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Errore durante la creazione della classe: {ex.Message}");
            }
        }

        private void BtnCaricaStudenti(object sender, RoutedEventArgs e)
        {
            if (comboBoxClassi.SelectedValue == null || comboBoxClassi.SelectedIndex == 0)
            {
                MessageBox.Show("Seleziona una classe prima di aggiungere uno studente.");
                return;
            }
            using SqlConnection connection = new SqlConnection(connectionStr);
            SqlCommand command = new SqlCommand("CaricaStudentiPerClasse", connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IdClasse", comboBoxClassi.SelectedValue);
            connection.Open();
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            System.Data.DataTable dataTable = new System.Data.DataTable();
            adapter.Fill(dataTable);
            if (dataTable.Rows.Count == 0)
            {
                MessageBox.Show("Non ci sono studenti per questa classe.");
                dataGridStudenti.ItemsSource = null;
            }
            else
            {
                dataGridStudenti.ItemsSource = dataTable.DefaultView;
            }
        }

        private void BtnCreaStudente(object sender, RoutedEventArgs e)
        {
            if (comboBoxClassi.SelectedValue == null || comboBoxClassi.SelectedIndex == 0)
            {
                MessageBox.Show("Seleziona una classe prima di creare uno studente.");
                return;
            }
            using SqlConnection connection = new SqlConnection(connectionStr);
            SqlCommand command = new SqlCommand("CreaStudente", connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Nome", txtNomeStudente.Text);
            command.Parameters.AddWithValue("@Cognome", txtCognomeStudente.Text);
            command.Parameters.AddWithValue("@IdClasse", comboBoxClassi.SelectedValue);
            connection.Open();
            try
            {
                command.ExecuteNonQuery();
                BtnCaricaStudenti(sender, e);
                MessageBox.Show("Studente creato con successo!");
                txtNomeStudente.Text = "";
                txtCognomeStudente.Text = "";
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Errore durante la creazione dello studente: {ex.Message}");
            }
        }

        private void BtnEliminaStudente(object sender, RoutedEventArgs e)
        {
            if (dataGridStudenti.SelectedItem is DataRowView drv)
            {
                int idStudente = (int)drv["IdStudente"];
                using SqlConnection connection = new SqlConnection(connectionStr);
                SqlCommand cmd = new("EliminaStudente", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdStudente", idStudente);

                connection.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                    BtnCaricaStudenti(sender, e);
                    MessageBox.Show("Studente rimosso!");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Errore durante la rimozione dello studente: {ex.Message}");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Seleziona uno studente da eliminare.");
            }
        }
    }
}
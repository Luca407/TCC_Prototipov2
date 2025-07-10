using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ProtótipoGerenciamentoEscola
{
    public partial class TelaProfessor : Form
    {
        private ConexaoBD conexao = new ConexaoBD();
        private readonly HttpClient _http = new HttpClient();
        private const string IBGE_ESTADOS_URL = "https://servicodados.ibge.gov.br/api/v1/localidades/estados";
        private const string VIACEP_URL = "https://viacep.com.br/ws/{0}/json/";

        public TelaProfessor()
        {
            InitializeComponent();
            this.Load += async (s, e) => await CarregarEstadosAsync();
            txtCEP.Leave += async (s, e) => await OnCepLeaveAsync();
        }

        private void btnFichaCadastral_Click(object sender, EventArgs e)
        {
            var telaAluno = new TelaPrincipal();
            telaAluno.ShowDialog();
            this.Hide();
        }

        private void btnTurmas_Click(object sender, EventArgs e)
        {
            var telaTurma = new TelaTurma();
            telaTurma.ShowDialog();
            this.Hide();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnCadastro_Click(object sender, EventArgs e)
        {
            var nomes = txtNome.Text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (nomes.Length < 2)
            {
                MessageBox.Show("Informe o nome completo (pelo menos dois nomes).", "Campo obrigatório",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNome.Focus();
                return;
            }

            if (!ValidarCpf(txtCPF.Text))
            {
                MessageBox.Show("CPF inválido.", "Erro de CPF",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCPF.Focus();
                return;
            }

            var cepNum = Regex.Replace(txtCEP.Text, @"\D", string.Empty);
            if (!string.IsNullOrWhiteSpace(txtCEP.Text) &&
                !Regex.IsMatch(cepNum, @"^\d{8}$"))
            {
                MessageBox.Show("CEP inválido. Se preencher, use 8 dígitos numéricos.", "Aviso de CEP",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCEP.Focus();
                return;
            }

            bool temCel = !string.IsNullOrWhiteSpace(txtTelCelular.Text);
            bool temRes = !string.IsNullOrWhiteSpace(txtTelResidencia.Text);
            if (!temCel && !temRes)
            {
                MessageBox.Show("Informe ao menos um telefone (celular ou residencial).", "Campo obrigatório",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTelCelular.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text) &&
                !Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("E-mail inválido.", "Erro de E-mail",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            int idEndereco = SalvarEndereco();
            int idContato = SalvarContato();

            var prof = new Professor
            {
                NomeCompleto = txtNome.Text.Trim(),
                CPF = txtCPF.Text,
                DataNascimento = dtpDataNascimento.Value,
                IdEndereco = idEndereco,
                IdContato = idContato
            };
            prof.Inserir();

            MessageBox.Show("Professor salvo com sucesso!", "Sucesso",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private async Task CarregarEstadosAsync()
        {
            try
            {
                var resp = await _http.GetStringAsync(IBGE_ESTADOS_URL);
                var estados = JsonConvert.DeserializeObject<List<EstadoIbge>>(resp);

                estados.Sort((a, b) => string.Compare(a.Sigla, b.Sigla, StringComparison.Ordinal));

                cbEstado.Items.Clear();
                foreach (var uf in estados)
                    cbEstado.Items.Add(uf.Sigla);

                cbEstado.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Não foi possível carregar estados: {ex.Message}", "Erro IBGE",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task OnCepLeaveAsync()
        {
            var cepRaw = txtCEP.Text;
            var cep = Regex.Replace(cepRaw, @"\D", string.Empty);

            if (string.IsNullOrWhiteSpace(cep))
                return;

            if (!Regex.IsMatch(cep, @"^\d{8}$"))
            {
                MessageBox.Show("CEP inválido. Deve ter 8 dígitos.", "Erro de CEP",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCEP.Focus();
                return;
            }

            LimparEndereco();
            try
            {
                var url = string.Format(VIACEP_URL, cep);
                var resp = await _http.GetStringAsync(url);
                var obj = JObject.Parse(resp);

                if (obj["erro"]?.Value<bool>() == true)
                {
                    MessageBox.Show("CEP não encontrado.", "Aviso",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                txtRua.Text = obj.Value<string>("logradouro") ?? string.Empty;
                txtBairro.Text = obj.Value<string>("bairro") ?? string.Empty;
                txtCidade.Text = obj.Value<string>("localidade") ?? string.Empty;
                var uf = obj.Value<string>("uf") ?? string.Empty;
                if (cbEstado.Items.Contains(uf))
                    cbEstado.SelectedItem = uf;

                txtNumero.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Falha ao buscar CEP: {ex.Message}", "Erro de Rede",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCEP.Focus();
            }
        }

        private void LimparEndereco()
        {
            txtRua.Clear();
            txtBairro.Clear();
            txtCidade.Clear();
            cbEstado.SelectedIndex = -1;
            txtNumero.Clear();
            txtComplemento.Clear();
        }

        private class EstadoIbge
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("nome")]
            public string Nome { get; set; } = string.Empty;

            [JsonProperty("sigla")]
            public string Sigla { get; set; } = string.Empty;
        }

        private int SalvarEndereco()
        {
            using var conn = conexao.Conectar();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Endereco (cep, rua, numero, bairro, cidade, estado, complemento)
                                 VALUES (@cep,@rua,@num,@bairro,@cid,@uf,@comp);
                                 SELECT LAST_INSERT_ID();";
            cmd.Parameters.AddWithValue("@cep",
                string.IsNullOrWhiteSpace(txtCEP.Text) ? (object)DBNull.Value : txtCEP.Text);
            cmd.Parameters.AddWithValue("@rua", txtRua.Text);
            cmd.Parameters.AddWithValue("@num", txtNumero.Text);
            cmd.Parameters.AddWithValue("@bairro", txtBairro.Text);
            cmd.Parameters.AddWithValue("@cid", txtCidade.Text);
            cmd.Parameters.AddWithValue("@uf",
                cbEstado.SelectedItem?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@comp", txtComplemento.Text);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private int SalvarContato()
        {
            using var conn = conexao.Conectar();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Contato (telefone_celular, telefone_residencial, email)
                                 VALUES (@cel,@res,@email);
                                 SELECT LAST_INSERT_ID();";
            cmd.Parameters.AddWithValue("@cel",
                string.IsNullOrWhiteSpace(txtTelCelular.Text) ? (object)DBNull.Value : txtTelCelular.Text);
            cmd.Parameters.AddWithValue("@res",
                string.IsNullOrWhiteSpace(txtTelResidencia.Text) ? (object)DBNull.Value : txtTelResidencia.Text);
            cmd.Parameters.AddWithValue("@email",
                string.IsNullOrWhiteSpace(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private bool ValidarCpf(string cpfRaw)
        {
            var cpf = Regex.Replace(cpfRaw, @"\D", string.Empty);
            return cpf.Length == 11;
        }

        private void btnPesquisar_Click(object sender, EventArgs e)
        {
            // Futuro: implementar pesquisa de professor
        }
    }
}

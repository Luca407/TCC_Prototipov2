using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtótipoGerenciamentoEscola
{
    internal class ConexaoBD
    {
        private string conexaoString = "Server=localhost; Database=teste; Uid=root; Pwd=;";

        private MySqlConnection? conexao = null;

        public MySqlConnection AbrirConexao()
        {
            try
            {
                if (conexao == null)
                    conexao = new MySqlConnection(conexaoString);

                if (conexao.State == System.Data.ConnectionState.Closed)
                    conexao.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao abrir a conexão com o banco de dados: " + ex.Message);
            }

            return conexao;
        }

        public void FecharConexao()
        {
            try
            {
                if (conexao != null && conexao.State == System.Data.ConnectionState.Open)
                    conexao.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao fechar a conexão com o banco de dados: " + ex.Message);
            }
        }
    }
}

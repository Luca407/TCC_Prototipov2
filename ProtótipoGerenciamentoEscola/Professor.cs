using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtótipoGerenciamentoEscola
{
    internal class Professor
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public int IdEndereco { get; set; }
        public int IdContato { get; set; }

        private ConexaoBD conexao = new ConexaoBD();

        public void Inserir()
        {
            string sql = @"INSERT INTO Professor 
                           (nome_completo, cpf, data_nascimento, id_endereco, id_contato) 
                           VALUES 
                           (@nome, @cpf, @data, @idEndereco, @idContato)";
            MySqlCommand cmd = new MySqlCommand(sql, conexao.AbrirConexao());
            cmd.Parameters.AddWithValue("@nome", NomeCompleto);
            cmd.Parameters.AddWithValue("@cpf", CPF);
            cmd.Parameters.AddWithValue("@data", DataNascimento);
            cmd.Parameters.AddWithValue("@idEndereco", IdEndereco);
            cmd.Parameters.AddWithValue("@idContato", IdContato);
            cmd.ExecuteNonQuery();
            conexao.FecharConexao();
        }

        public void Atualizar()
        {
            string sql = @"UPDATE Professor SET 
                            nome_completo = @nome,
                            cpf = @cpf,
                            data_nascimento = @data,
                            id_endereco = @idEndereco,
                            id_contato = @idContato
                          WHERE id_professor = @id";
            MySqlCommand cmd = new MySqlCommand(sql, conexao.AbrirConexao());
            cmd.Parameters.AddWithValue("@nome", NomeCompleto);
            cmd.Parameters.AddWithValue("@cpf", CPF);
            cmd.Parameters.AddWithValue("@data", DataNascimento);
            cmd.Parameters.AddWithValue("@idEndereco", IdEndereco);
            cmd.Parameters.AddWithValue("@idContato", IdContato);
            cmd.Parameters.AddWithValue("@id", Id);
            cmd.ExecuteNonQuery();
            conexao.FecharConexao();
        }

        public void Excluir()
        {
            string sql = "DELETE FROM Professor WHERE id_professor = @id";
            MySqlCommand cmd = new MySqlCommand(sql, conexao.AbrirConexao());
            cmd.Parameters.AddWithValue("@id", Id);
            cmd.ExecuteNonQuery();
            conexao.FecharConexao();
        }

        public DataTable ListarTodos()
        {
            string sql = "SELECT * FROM Professor";
            MySqlCommand cmd = new MySqlCommand(sql, conexao.AbrirConexao());
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable tabela = new DataTable();
            da.Fill(tabela);
            conexao.FecharConexao();
            return tabela;
        }

        public DataTable PesquisarPorNome(string nome)
        {
            string sql = "SELECT * FROM Professor WHERE nome_completo LIKE @nome";
            MySqlCommand cmd = new MySqlCommand(sql, conexao.AbrirConexao());
            cmd.Parameters.AddWithValue("@nome", "%" + nome + "%");
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable tabela = new DataTable();
            da.Fill(tabela);
            conexao.FecharConexao();
            return tabela;


        }

        

        // Classe para desserializar estados do IBGE
        

    }
} // Fim da classe Professor
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;

namespace eAgenda.WinApp.ModuloContato
{
    internal class RepositorioContatoEmSQL : IRepositorioContato
    {
        private string enderecoBanco;

        public RepositorioContatoEmSQL()
        {
            //Connection string
            enderecoBanco = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=eAgendaDb;Integrated Security=True;Pooling=False";
        }
        public void Cadastrar(Contato novoContato)
        {
            string SQLInserir =
                @"INSERT INTO [TBCONTATO]
                    (
                        [NOME],
                        [EMAIL],
                        [TELEFONE],
                        [EMPRESA],
                        [CARGO]
                    )
                    VALUES
                    (
                        @NOME,   
                        @EMAIL,   
                        @TELEFONE,  
                        @EMPRESA,
                        @CARGO
                    ); SELECT SCOPE_IDENTITY();"; //SELECT SCOPE_IDENTIFY(); RETORNA O ID

            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco); //conexão com o bando endereçãndo o endereço do banco

            SqlCommand comandoInsercao = new SqlCommand(SQLInserir, conexaoComBanco); //Criou o objeto para dar o comando de inserção.

            ConfigurarParametrosContato(novoContato, comandoInsercao);  

            conexaoComBanco.Open(); //Abriu conexão com o banco. SEMPRE FECHAR A CONEXÃO

            object id = comandoInsercao.ExecuteScalar(); //Executa a Query e retorna um objeto com o primeiro dado da primeira coluna, no caso vai retornar ID
            
            novoContato.Id = Convert.ToInt32(id); //O objeto precisa ser convertido para um int, ou se for uma string, para uma string.

            conexaoComBanco.Close(); //Fechou a conexão com o banco.
        }

        public bool Editar(int id, Contato contatoEditado)
        {
            string SQLEditar =
                @"UPDATE [TBCONTATO]
                    SET
                        [NOME] = @NOME,
                        [EMAIL] = @EMAIL,
                        [TELEFONE] = @TELEFONE,
                        [EMPRESA] = @EMPRESA,
                        [CARGO] = @CARGO
                    WHERE
                        [ID] = @ID";

            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco); //conexão com o bando endereçãndo o endereço do banco

            SqlCommand comandoEdicao = new SqlCommand(SQLEditar, conexaoComBanco); //Criou o objeto para dar o comando de edição.

            contatoEditado.Id = id;

            ConfigurarParametrosContato(contatoEditado, comandoEdicao);

            conexaoComBanco.Open(); //Abriu conexão com o banco. SEMPRE FECHAR A CONEXÃO

            int numeroRegistrosAfetados = comandoEdicao.ExecuteNonQuery(); //Retorna quantos numeros de registros foram afetados

            conexaoComBanco.Close(); //Fechou a conexão com o banco.

            if(numeroRegistrosAfetados < 1)
                return false;

            return true;
        }

        public bool Excluir(int id)
        {
            string SQLExcluir =
                @"DELETE FROM [TBCONTATO]
                    WHERE
                        [ID] = @ID";

            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco); //conexão com o bando endereçãndo o endereço do banco

            SqlCommand comandoExclusao = new SqlCommand(SQLExcluir, conexaoComBanco); //Criou o objeto para dar o comando de apagar.

            comandoExclusao.Parameters.AddWithValue("ID", id);

            conexaoComBanco.Open();

            int numeroRegistrosExcluidos = comandoExclusao.ExecuteNonQuery();

            conexaoComBanco.Close();

            if(numeroRegistrosExcluidos < 1) 
                return false;

            return true;
        }

        public Contato SelecionarPorId(int idSelecionado)
        {
            string SQLSelecionarPorId =
               @"SELECT
                    [ID],
                    [NOME],
                    [EMAIL],
                    [TELEFONE],
                    [EMPRESA],
                    [CARGO]
                FROM
                    [TBCONTATO]
                WHERE
                    [ID] = @ID";

            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco); //conexão com o bando endereçãndo o endereço do banco

            SqlCommand comandoSelecao = new SqlCommand(SQLSelecionarPorId, conexaoComBanco); //Criou o objeto para dar o comando de selecionar todos.

            comandoSelecao.Parameters.AddWithValue("ID", idSelecionado);

            conexaoComBanco.Open(); //Abriu conexão com o banco. SEMPRE FECHAR A CONEXÃO

            SqlDataReader leitor = comandoSelecao.ExecuteReader();

            Contato contato = null;

            if (leitor.Read())
                contato = ConverterParaContato(leitor);

            conexaoComBanco.Close(); //Fechou a conexão com o banco.

            return contato;
        }

        public List<Contato> SelecionarTodos()
        {
            string SQLSelecionarTodos =
               @"SELECT
                    [ID],
                    [NOME],
                    [EMAIL],
                    [TELEFONE],
                    [EMPRESA],
                    [CARGO]
                FROM
                    [TBCONTATO]";

            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco); //conexão com o bando endereçãndo o endereço do banco

            SqlCommand comandoSelecao = new SqlCommand(SQLSelecionarTodos, conexaoComBanco); //Criou o objeto para dar o comando de selecionar todos.

            conexaoComBanco.Open(); //Abriu conexão com o banco. SEMPRE FECHAR A CONEXÃO

            SqlDataReader leitorContato = comandoSelecao.ExecuteReader();
            //Executou o ExecuteReader(); para elr os dados, Todas as informações solicitadas em SQLSelecionarTodos estará dentro do leitorContato

            List<Contato> contatos = new List<Contato>();

            while (leitorContato.Read())
            {
                Contato contato = ConverterParaContato(leitorContato);
                
                contatos.Add(contato);
            }

            conexaoComBanco.Close(); //Fechou a conexão com o banco.
            
            return contatos;
        }

        private Contato ConverterParaContato(SqlDataReader leitor)
        {
            Contato contato = new Contato()
            {
                Id = Convert.ToInt32(leitor["ID"]),
                Nome = Convert.ToString(leitor["NOME"]),
                Email = Convert.ToString(leitor["EMAIL"]),
                Telefone = Convert.ToString(leitor["TELEFONE"]),
                Empresa = Convert.ToString(leitor["EMPRESA"]),
                Cargo = Convert.ToString(leitor["CARGO"]),
            };
            
            return contato;
        }

        private void ConfigurarParametrosContato(Contato contato, SqlCommand comando)
        {
            //AddWithValue ou seja, ADICIONE COM O VALOR, LOGO, SUBSTITUI O NOME PELA VARIAVEL

            comando.Parameters.AddWithValue("ID", contato.Id);
            comando.Parameters.AddWithValue("NOME", contato.Nome);
            comando.Parameters.AddWithValue("EMAIL", contato.Email);
            comando.Parameters.AddWithValue("TELEFONE", contato.Telefone);
            comando.Parameters.AddWithValue("EMPRESA", contato.Empresa);
            comando.Parameters.AddWithValue("CARGO", contato.Cargo);
        }
    }
}

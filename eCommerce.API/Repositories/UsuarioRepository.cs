using eCommerce.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace eCommerce.API.Repositories
{
    
    public class UsuarioRepository : IUsuarioRepository
    {
        private IDbConnection _connection;
        public UsuarioRepository()
        {
            _connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=eCommerce;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }
        public List<Usuario> Get()
        {
            #region SEM RELACIONAMENTO
            //return _connection.Query<Usuario>("SELECT * FROM Usuarios").ToList();
            #endregion
            //COM RELACIONAMENTO TRATANDO DUPLICIDADE DE LINHAS
            List<Usuario> lstUsuarios = new List<Usuario>();
            //string sql = "SELECT * FROM Usuarios U LEFT JOIN Contatos C ON C.UsuarioId = U.Id LEFT JOIN EnderecosEntrega E ON E.UsuarioId = U.Id;";
            //M P/ M
            string sql = "SELECT U.*, C.*, E.*, D.* FROM Usuarios U LEFT JOIN Contatos C ON C.UsuarioId = U.Id LEFT JOIN EnderecosEntrega E ON E.UsuarioId = U.Id LEFT JOIN UsuariosDepartamentos UD ON UD.UsuarioId = U.Id LEFT JOIN Departamentos D ON UD.DepartamentoId = D.Id;";
            _connection.Query<Usuario, Contato, EnderecoEntrega, Departamento, Usuario>(sql,
                (usuario, contato, enderecoEntrega, departamento) =>
                {
                    //verificação do usuario
                    if (lstUsuarios.SingleOrDefault(a => a.Id == usuario.Id) == null)
                    {
                        usuario.Departamentos = new List<Departamento>();
                        usuario.EnderecosEntrega = new List<EnderecoEntrega>();
                        usuario.Contato = contato;
                        lstUsuarios.Add(usuario);
                    }
                    else
                    {
                        usuario = lstUsuarios.SingleOrDefault(a => a.Id == usuario.Id);
                    }
                    //verificação do endereço
                    if (usuario.EnderecosEntrega.SingleOrDefault(a => a.Id == enderecoEntrega.Id) == null)
                    {
                        usuario.EnderecosEntrega.Add(enderecoEntrega);
                    }

                    //verificação do departamento
                    if (usuario.Departamentos.SingleOrDefault(a => a.Id == departamento.Id) == null)
                    {
                        usuario.Departamentos.Add(departamento);
                    }

                    return usuario;
                });
            return lstUsuarios;
        }

        public Usuario Get(int id)
        {
            //SEM RELACIONAMENTO return _connection.QuerySingleOrDefault<Usuario>("SELECT * FROM Usuarios WHERE Id = @Id", new { Id = id });
            //COM RELACIONAMENTO
            //DENTRO DE QUERY<> PODEM TER ATE 7 TABELAS, ULTIMO PARAMETRO É O TIPO DE RETORNO
            List<Usuario> lstUsuarios = new List<Usuario>();
            //string sql = "SELECT * FROM Usuarios U LEFT JOIN Contatos C ON C.UsuarioId = U.Id LEFT JOIN EnderecosEntrega E ON E.UsuarioId = U.Id WHERE U.Id = @Id;";
            //M P/ M
            string sql = "SELECT U.*, C.*, E.*, D.* FROM Usuarios U LEFT JOIN Contatos C ON C.UsuarioId = U.Id LEFT JOIN EnderecosEntrega E ON E.UsuarioId = U.Id LEFT JOIN UsuariosDepartamentos UD ON UD.UsuarioId = U.Id LEFT JOIN Departamentos D ON UD.DepartamentoId = D.Id WHERE U.Id = @Id;";
            _connection.Query<Usuario, Contato, EnderecoEntrega, Departamento, Usuario>(sql,
                (usuario, contato, enderecoEntrega, departamento) =>
                {
                    if (lstUsuarios.SingleOrDefault(a => a.Id == usuario.Id) == null)
                    {
                        usuario.Departamentos = new List<Departamento>();
                        usuario.EnderecosEntrega = new List<EnderecoEntrega>();
                        usuario.Contato = contato;
                        lstUsuarios.Add(usuario);
                    }
                    else
                    {
                        usuario = lstUsuarios.SingleOrDefault(a => a.Id == usuario.Id);

                    }
                    //verificação do endereço
                    if (usuario.EnderecosEntrega.SingleOrDefault(a => a.Id == enderecoEntrega.Id) == null)
                    {
                        usuario.EnderecosEntrega.Add(enderecoEntrega);
                    }

                    //verificação do departamento
                    if (usuario.Departamentos.SingleOrDefault(a => a.Id == departamento.Id) == null)
                    {
                        usuario.Departamentos.Add(departamento);
                    }

                    
                    return usuario;
                }, new { Id = id});

            return lstUsuarios.SingleOrDefault();
        }

        public void Insert(Usuario usuario)
        {
            #region SEM RELACIONAMENTO
            //string sql = "INSERT INTO Usuarios (Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro)" +
            //    "VALUES (@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro)" +
            //    "SELECT CAST(SCOPE_iDENTITY() AS INT)";
            //usuario.Id = _connection.Query<int>(sql, usuario).Single();
            #endregion

            //COM RELACIONAMENTO
            _connection.Open();
            var transaction = _connection.BeginTransaction();

            try
            {
                string sql = "INSERT INTO Usuarios (Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro)" +
                "VALUES (@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro);" +
                "SELECT CAST(SCOPE_iDENTITY() AS INT);";
                usuario.Id = _connection.Query<int>(sql, usuario, transaction).Single();

                if (usuario.Contato != null)
                {
                    usuario.Contato.UsuarioId = usuario.Id;
                    string sqlContato = "INSERT INTO Contatos(UsuarioId, Telefone, Celular)" +
                        "VALUES (@UsuarioId, @Telefone, @Celular);SELECT CAST(SCOPE_iDENTITY() AS INT);";
                    usuario.Contato.Id = _connection.Query<int>(sqlContato, usuario.Contato, transaction).Single();
                }

                if (usuario.EnderecosEntrega != null && usuario.EnderecosEntrega.Count > 0)
                {
                    foreach (var enderecoEntrega in usuario.EnderecosEntrega)
                    {
                        enderecoEntrega.UsuarioId = usuario.Id;
                        string sqlEndereco = "INSERT INTO EnderecosEntrega (UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento)" +
                            "VALUES (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento); SELECT CAST(SCOPE_iDENTITY() AS INT);";
                        enderecoEntrega.Id = _connection.Query<int>(sqlEndereco, enderecoEntrega, transaction).Single();
                    }
                }

                if (usuario.Departamentos != null && usuario.Departamentos.Count > 0)
                {
                    foreach (var departamento in usuario.Departamentos)
                    {
                        string sqlUsuarioDepartamentos = "INSERT INTO UsuariosDepartamentos (UsuarioId, DepartamentoId) VALUES (@UsuarioId, @DepartamentoId);";
                        _connection.Execute(sqlUsuarioDepartamentos,new { UsuarioId = usuario.Id, DepartamentoId = departamento.Id}, transaction);
                    }
                }
                transaction.Commit();
            }
            catch (Exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception)
                {
                    //mensagem
                    throw;
                }
                
            }
            finally
            {
                _connection.Close();
            }
            
        }

        public void Update(Usuario usuario)
        {
            #region SEM RELACIONAMENTO
            //string sql = "UPDATE Usuarios SET Nome =  @Nome,Email = @Email,Sexo = @Sexo,RG = @RG," +
            //    "CPF = @CPF,Nomemae = @NomeMae,SituacaoCadastro = @SituacaoCadastro,DataCadastro = @DataCadastro" +
            //    " WHERE Id = @Id";
            //_connection.Execute(sql, usuario);
            #endregion
            _connection.Open();
            var transaction = _connection.BeginTransaction();

            try
            {
                //COM RELACIONAMENTO
                string sql = "UPDATE Usuarios SET Nome =  @Nome,Email = @Email,Sexo = @Sexo,RG = @RG," +
                    "CPF = @CPF,Nomemae = @NomeMae,SituacaoCadastro = @SituacaoCadastro,DataCadastro = @DataCadastro" +
                    " WHERE Id = @Id;";
                _connection.Execute(sql, usuario, transaction);

                if (usuario.Contato != null)
                {
                    string sqlContato = "UPDATE Contatos SET UsuarioId  = @UsuarioId, Telefone = @Telefone, Celular = @Celular  WHERE Id = @Id;";
                    _connection.Execute(sqlContato, usuario.Contato, transaction);
                }

                string deletarEnderecoEntrega = "DELETE FROM EnderecosEntrega WHERE UsuarioId = @Id";
                _connection.Execute(deletarEnderecoEntrega, usuario, transaction);


                if (usuario.EnderecosEntrega != null && usuario.EnderecosEntrega.Count > 0)
                {
                    foreach (var enderecoEntrega in usuario.EnderecosEntrega)
                    {
                        enderecoEntrega.UsuarioId = usuario.Id;
                        string sqlEndereco = "INSERT INTO EnderecosEntrega (UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento)" +
                            "VALUES (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento); SELECT CAST(SCOPE_iDENTITY() AS INT);";
                        enderecoEntrega.Id = _connection.Query<int>(sqlEndereco, enderecoEntrega, transaction).Single();
                    }
                }

                string deletarUsuariosDepartamentos = "DELETE FROM UsuariosDepartamentos WHERE UsuarioId = @Id";
                _connection.Execute(deletarUsuariosDepartamentos, usuario, transaction);


                if (usuario.Departamentos != null && usuario.Departamentos.Count > 0)
                {
                    foreach (var departamento in usuario.Departamentos)
                    {
                        string sqlUsuarioDepartamentos = "INSERT INTO UsuariosDepartamentos (UsuarioId, DepartamentoId) VALUES (@UsuarioId, @DepartamentoId);";
                        _connection.Execute(sqlUsuarioDepartamentos, new { UsuarioId = usuario.Id, DepartamentoId = departamento.Id }, transaction);
                    }
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception)
                {

                    throw;
                }

            }
            finally
            {
                _connection.Close();
            }

        }

        public void Delete(int id)
        {
            //CASO ON DELETE CASCADE NÃO ESTIVE NO BANCO
            //DELETAR EM ORDEM AS CHAVES ESTRANGEIRAS VINCULADAS AO IDUSUARIO, PARA POR ULTIMO DELETAR O USUARIO.
            _connection.Execute("DELETE FROM Usuarios WHERE Id = @Id", new { Id = id });
            

        }

        //private static List<Usuario> _db = new List<Usuario>()
        //{
        //    new Usuario(){Id=1, Nome="Flavio Leandro", Email="leans.flavio@teste.com"},
        //    new Usuario(){Id=2, Nome="Girlene Marques", Email="girlene@teste.com"},
        //    new Usuario(){Id=3, Nome="Pereira Silva", Email="pereira.silva@teste.com"},
        //};

    }
}

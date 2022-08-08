﻿using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Dapper.FluentMap;
namespace eCommerce.API.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Usuarios")]
    public class UsuarioTwo
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Cod { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Sexo { get; set; }
        public string RG { get; set; }
        public string CPF { get; set; }
        public string NomeCompletoMae { get; set; }
        public string Situacao { get; set; }
        public DateTimeOffset DataCadastro { get; set; }

        [Write(false)]
        public Contato Contato { get; set; }
        [Write(false)]
        public ICollection<EnderecoEntrega> EnderecosEntrega { get; set; }
        [Write(false)]
        public ICollection<Departamento> Departamentos { get; set; }

    }
}

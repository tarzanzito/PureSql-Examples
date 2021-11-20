using System.Collections;
using System.Collections.Generic;

namespace Candal.Models
{
    public class Regiao
    {
        public int IdRegiao { get; set; }
        public string CodRegiao { get; set; }
        public string NomeRegiao { get; set; }
        public List<Estado> Estados { get; set; }
    }
}

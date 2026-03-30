using LIB_Auditoria;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_Conta_SaldoExtrato.Helpers
{
    [ExcludeFromCodeCoverage]
    public class MascaraDadosSensiveis
    {

        public AuditoriaDto MascararDados(AuditoriaDto auditoriaDto)
        {
            return auditoriaDto;
        }

    }
}

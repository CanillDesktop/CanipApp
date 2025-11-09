using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Enums
{
    public enum PublicoAlvoMedicamentoEnum
    {
        Animal,
        [Description("Humano e Animal")]
        HumanoEAnimal
    }
}

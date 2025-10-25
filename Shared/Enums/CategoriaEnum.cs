using System.ComponentModel;

namespace Shared.Enums
{
    public enum CategoriaEnum
    {
        [Description("Acessórios")]
        ACESSORIO = 1,
        [Description("Alimentação")]
        ALIMENTO,
        [Description("Diversos")]
        DIVERSOS,
        [Description("Eletrônicos")]
        ELETRONICOS,
        [Description("Equipamentos Cirúrgicos")]
        EQUIPAMENTO_CIRURGICO,
        [Description("Higiene")]
        HIGIENE,
        [Description("Instrumentais")]
        INSTRUMENTAIS,
        [Description("Limpeza")]
        LIMPEZA,
        [Description("Material Hospitalar")]
        MATERIAL_HOSPITALAR,
        [Description("Outros")]
        OUTROS,
        [Description("Outros Equipamentos")]
        OUTROS_EQUIPAMENTOS,
        [Description("Vestuário")]
        VESTUARIO,
        [Description("Veterinário")]
        VETERINARIO
    }
}

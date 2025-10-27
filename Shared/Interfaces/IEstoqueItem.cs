using System;

namespace Shared.Interfaces
{
    public interface IEstoqueItem
    {
        string Nome { get; }
        int QuantidadeAtual { get; }
        int NivelMinimoEstoque { get; set; }
        DateTime? DataDeValidade { get; }
    }
}

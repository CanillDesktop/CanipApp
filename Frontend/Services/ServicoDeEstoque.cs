using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Frontend.Services
{
    public class ServicoDeEstoque
    {
        public List<IEstoqueItem> VerificarEstoqueBaixo(List<IEstoqueItem> todosOsItens)
            => todosOsItens.Where(p => p.QuantidadeAtual <= p.NivelMinimoEstoque).ToList();

        public List<IEstoqueItem> VerificarVencimentoProximo(List<IEstoqueItem> todosOsItens, int diasParaAlerta = 30)
        {
            var hoje = DateTime.Today;
            var dataLimite = hoje.AddDays(diasParaAlerta);

            return todosOsItens
                .Where(p => p.DataDeValidade.HasValue && p.DataDeValidade.Value.Date > hoje && p.DataDeValidade.Value.Date <= dataLimite)
                .OrderBy(p => p.DataDeValidade)
                .ToList();
        }
    }
}

using Backend.Exceptions;
using Backend.Models.Insumos;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Shared.DTOs.Insumos;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Backend.Services
{
    public class InsumosService : IInsumosService
    {
        public readonly IInsumosRepository _repository;

        public InsumosService(IInsumosRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<InsumosLeituraDTO>> BuscarTodosAsync()
        {
            // 1. Busca do Repositório (Dados crus do banco)
            var dadosDoBanco = await _repository.GetAsync();

            // DEBUG: Veja no Output quantos itens vieram
            var quantidade = dadosDoBanco.Count();
            System.Diagnostics.Debug.WriteLine($"[Service] Itens vindos do Repo: {quantidade}");

            foreach (var item in dadosDoBanco)
            {
                System.Diagnostics.Debug.WriteLine($"[Service] Item ID: {item.IdItem} - Nome: {item.DescricaoSimplificada}");
            }

            // 2. Converte para DTO (Aqui pode estar o erro do "Implicit Operator")
            var listaRetorno = dadosDoBanco.Select(p =>
            {
                var dto = (InsumosLeituraDTO)p;
                // DEBUG: Verifique se a conversão manteve o ID correto
                System.Diagnostics.Debug.WriteLine($"[Service] DTO Convertido ID: {dto.IdItem}");
                return dto;
            });

            return listaRetorno;
        }

        public async Task<InsumosLeituraDTO?> BuscarPorIdAsync(int id) => (await _repository.GetByIdAsync(id))!;

        public async Task<InsumosLeituraDTO?> CriarAsync(InsumosCadastroDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CodInsumo)
                || string.IsNullOrWhiteSpace(dto.DescricaoSimplificada)
                || !Enum.IsDefined(typeof(UnidadeInsumosEnum), (int)dto.Unidade)
                || string.IsNullOrWhiteSpace(dto.Lote))
        {
                throw new ModelIncompletaException("Um ou mais campos obrigatórios não foram preenchidos");
            }

            return await _repository.CreateAsync(dto);
        }

        public async Task<InsumosLeituraDTO?> AtualizarAsync(InsumosCadastroDTO dto)
        {
            try
            {
                Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Debug.WriteLine($"[InsumosService] 🔄 Atualizando insumo: {dto.CodInsumo}");

                // ════════════════════════════════════════════════════════════
                // 1. BUSCAR INSUMO EXISTENTE (com lotes e nível de estoque)
                // ════════════════════════════════════════════════════════════
                // ⚠️ IMPORTANTE: Precisa ter um método no repository que aceite int
                // Se não tiver, você precisa usar o DTO para identificar qual atualizar

                // OPÇÃO A: Se você tem IdItem no DTO
                var insumoExistente = await _repository.GetByIdAsync(dto.CodigoId);

                // OPÇÃO B: Se não tem IdItem, buscar por CodInsumo
                // var todosInsumos = await _repository.GetAsync();
                // var insumoExistente = todosInsumos.FirstOrDefault(i => i.CodInsumo == dto.CodInsumo);

                if (insumoExistente == null)
                {
                    Debug.WriteLine($"[InsumosService] ❌ Insumo não encontrado");
                    throw new InvalidOperationException($"Insumo com código {dto.CodInsumo} não foi encontrado");
                }

                Debug.WriteLine($"[InsumosService] ✅ Insumo encontrado: ID={insumoExistente.IdItem}");

                // ════════════════════════════════════════════════════════════
                // 2. ATUALIZAR CAMPOS ESCALARES
                // ════════════════════════════════════════════════════════════
                insumoExistente.CodInsumo = dto.CodInsumo;
                insumoExistente.DescricaoSimplificada = dto.DescricaoSimplificada;
                insumoExistente.DescricaoDetalhada = dto.DescricaoDetalhada;
                insumoExistente.Unidade = dto.Unidade;

                Debug.WriteLine($"[InsumosService] ✅ Campos escalares atualizados");

                // ════════════════════════════════════════════════════════════
                // 3. ATUALIZAR OU ADICIONAR LOTE
                // ════════════════════════════════════════════════════════════
                if (!string.IsNullOrWhiteSpace(dto.Lote))
                {
                    var loteExistente = insumoExistente.ItensEstoque
                        ?.FirstOrDefault(e => e.Lote == dto.Lote);

                    if (loteExistente != null)
                    {
                        // ✅ ATUALIZAR LOTE EXISTENTE
                        Debug.WriteLine($"[InsumosService] ♻️  Atualizando lote existente: {dto.Lote}");
                        Debug.WriteLine($"   Quantidade: {loteExistente.Quantidade} → {dto.Quantidade}");

                        loteExistente.Quantidade = dto.Quantidade;
                        loteExistente.DataEntrega = dto.DataEntrega;
                        loteExistente.DataValidade = dto.DataValidade;
                        loteExistente.NFe = dto.NFe;

                        // 🔥 ATUALIZAR TIMESTAMP DO LOTE
                        loteExistente.DataHoraInsercaoRegistro = DateTime.UtcNow;

                        Debug.WriteLine($"   DataHoraInsercaoRegistro: {loteExistente.DataHoraInsercaoRegistro}");
                    }
                    else
                    {
                        // ✅ ADICIONAR NOVO LOTE
                        Debug.WriteLine($"[InsumosService] ➕ Adicionando novo lote: {dto.Lote}");
                        Debug.WriteLine($"   Quantidade: {dto.Quantidade}");

                        var novoLote = new ItemEstoqueModel
                        {
                            IdItem = insumoExistente.IdItem,
                            CodItem = insumoExistente.CodInsumo,
                            Lote = dto.Lote,
                            Quantidade = dto.Quantidade,
                            DataEntrega = dto.DataEntrega,
                            DataValidade = dto.DataValidade,
                            NFe = dto.NFe,
                            DataHoraInsercaoRegistro = DateTime.UtcNow
                        };

                        if (insumoExistente.ItensEstoque == null)
                        {
                            insumoExistente.ItensEstoque = new List<ItemEstoqueModel>();
                        }

                        insumoExistente.ItensEstoque.Add(novoLote);

                        Debug.WriteLine($"   DataHoraInsercaoRegistro: {novoLote.DataHoraInsercaoRegistro}");
                    }

                    var quantidadeTotal = insumoExistente.ItensEstoque?.Sum(x => x.Quantidade) ?? 0;

                    if (quantidadeTotal <= 0)
                    {
                        // Marca como deletado
                        insumoExistente.IsDeleted = true;
                        Debug.WriteLine($"[Service] 🗑️ Estoque zerado (Total: {quantidadeTotal}). Marcando {insumoExistente.CodInsumo} como deletado.");
                    }
                    else
                    {
                        // Garante que, se aumentou o estoque, ele deixa de ser deletado (Opcional, mas seguro)
                        insumoExistente.IsDeleted = false;
                    }
                }

                // ════════════════════════════════════════════════════════════
                // 4. ATUALIZAR NÍVEL DE ESTOQUE
                // ════════════════════════════════════════════════════════════
                if (insumoExistente.ItemNivelEstoque != null)
                {
                    insumoExistente.ItemNivelEstoque.NivelMinimoEstoque = dto.NivelMinimoEstoque;
                    Debug.WriteLine($"[InsumosService] ✅ Nível mínimo atualizado: {dto.NivelMinimoEstoque}");
                }

                // ════════════════════════════════════════════════════════════
                // 5. 🔥 CRÍTICO: ATUALIZAR TIMESTAMP DA ENTIDADE PAI
                // ════════════════════════════════════════════════════════════
                insumoExistente.DataAtualizacao = DateTime.UtcNow;
             
                Debug.WriteLine($"[InsumosService] 🔥 DataAtualizacao atualizado: {insumoExistente.DataAtualizacao}");

                // ════════════════════════════════════════════════════════════
                // 6. SALVAR VIA REPOSITORY
                // ════════════════════════════════════════════════════════════
                var resultado = await _repository.UpdateAsync(insumoExistente);

                Debug.WriteLine($"[InsumosService] ✅ Insumo salvo com sucesso!");
                Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                return (InsumosLeituraDTO)resultado;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InsumosService] ❌ Erro ao atualizar insumo: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> DeletarAsync(int id) => await _repository.DeleteAsync(id);

        public async Task<IEnumerable<InsumosLeituraDTO>> BuscarTodosAsync(InsumosFiltroDTO filtro) => (await _repository.GetAsync(filtro)).Select(p => (InsumosLeituraDTO)p);
    }
}
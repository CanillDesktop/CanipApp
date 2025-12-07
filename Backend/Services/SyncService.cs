using Amazon.DynamoDBv2.DataModel;
using Backend.Context;
using Backend.Helpers;
using Backend.Models;
using Backend.Models.Insumos;
using Backend.Models.Medicamentos;
using Backend.Models.Produtos;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Backend.Services
{
    public class SyncService : ISyncService
    {
        private readonly IDynamoDBContext _dynamoDBContext;
        private readonly CanilAppDbContext _localDbContext;

        public SyncService(IDynamoDBContext dynamoDBContext, CanilAppDbContext localDbContext)
        {
            _dynamoDBContext = dynamoDBContext;
            _localDbContext = localDbContext;
        }

        public async Task SincronizarTabelasAsync()
        {
            await SincronizarMedicamentosAsync();
            await SincronizarProdutosAsync();
            await SincronizarInsumosAsync();
            await SincronizarRetiradaEstoqueAsync();
        }

        public async Task LimparRegistrosExcluidosAsync()
        {
            await SincronizarTabelasAsync();
            await LimparMedicamentosExcluidosAsync();
            await LimparProdutosExcluidosAsync();
            await LimparInsumosExcluidosAsync();
            await LimparRetiradaEstoqueExcluidosAsync();
        }

        // ==========================================================================================
        // 🛠️ MÉTODOS AUXILIARES GENÉRICOS (Reutilizáveis para Insumos, Medicamentos e Produtos)
        // ==========================================================================================

        /// <summary>
        /// Corrige o problema do SQLite retornar 'Unspecified', o que faz o Dynamo somar +3h erradamente.
        /// </summary>
        private void GarantirUtcLocal<T>(T item) where T : ItemComEstoqueBaseModel
        {
            // Data de Inserção Base
            item.DataHoraInsercaoRegistro = DateTime.SpecifyKind(item.DataHoraInsercaoRegistro, DateTimeKind.Utc);

            // Tenta achar a DataAtualizacao (que não está na base, mas nas classes filhas) via Reflection ou Cast
            if (item is InsumosModel i) i.DataAtualizacao = DateTime.SpecifyKind(i.DataAtualizacao, DateTimeKind.Utc);
            if (item is MedicamentosModel m) m.DataAtualizacao = DateTime.SpecifyKind(m.DataAtualizacao, DateTimeKind.Utc);
            if (item is ProdutosModel p) p.DataAtualizacao = DateTime.SpecifyKind(p.DataAtualizacao, DateTimeKind.Utc);

            // Nível Estoque
            if (item.ItemNivelEstoque != null)
                item.ItemNivelEstoque.DataHoraInsercaoRegistro = DateTime.SpecifyKind(item.ItemNivelEstoque.DataHoraInsercaoRegistro, DateTimeKind.Utc);

            // Lotes
            if (item.ItensEstoque != null)
            {
                foreach (var lote in item.ItensEstoque)
                {
                    lote.DataHoraInsercaoRegistro = DateTime.SpecifyKind(lote.DataHoraInsercaoRegistro, DateTimeKind.Utc);
                    lote.DataEntrega = DateTime.SpecifyKind(lote.DataEntrega, DateTimeKind.Utc);
                    if (lote.DataValidade.HasValue)
                        lote.DataValidade = DateTime.SpecifyKind(lote.DataValidade.Value, DateTimeKind.Utc);
                }
            }
        }

        /// <summary>
        /// Mescla os lotes (ItensEstoque) comparando as datas individuais de cada lote.
        /// Funciona para Insumos, Medicamentos e Produtos.
        /// </summary>
        private bool MesclarLotes<T>(T destino, T origem, bool ehDestinoLocal) where T : ItemComEstoqueBaseModel
        {
            var lotesDestinoCollection = ehDestinoLocal
                ? destino.ItensEstoque
                : destino.ItensEstoqueDynamo ?? new List<ItemEstoqueModel>();

            var lotesOrigemCollection = ehDestinoLocal
                ? origem.ItensEstoqueDynamo ?? new List<ItemEstoqueModel>()
                : origem.ItensEstoque;

            var lotesDestinoMap = lotesDestinoCollection.ToDictionary(l => l.Lote ?? string.Empty, l => l);
            var lotesParaAdicionar = new List<ItemEstoqueModel>();
            bool houveMudancas = false;

            foreach (var loteOrigem in lotesOrigemCollection)
            {
                var chaveLote = loteOrigem.Lote ?? string.Empty;

                if (lotesDestinoMap.TryGetValue(chaveLote, out var loteDestino))
                {
                    // LOTE EXISTE EM AMBOS - Vence o mais recente
                    if (loteOrigem.DataHoraInsercaoRegistro > loteDestino.DataHoraInsercaoRegistro)
                    {
                        // Atualizar dados do lote destino com os do lote origem
                        loteDestino.Quantidade = loteOrigem.Quantidade;
                        loteDestino.DataEntrega = loteOrigem.DataEntrega;
                        loteDestino.DataValidade = loteOrigem.DataValidade;
                        loteDestino.NFe = loteOrigem.NFe;
                        loteDestino.DataHoraInsercaoRegistro = loteOrigem.DataHoraInsercaoRegistro;
                        houveMudancas = true;
                    }
                }
                else
                {
                    // LOTE SÓ NA ORIGEM - Adicionar
                    var novoLote = new ItemEstoqueModel
                    {
                        IdItem = destino.IdItem,
                        CodItem = loteOrigem.CodItem,
                        Lote = loteOrigem.Lote,
                        Quantidade = loteOrigem.Quantidade,
                        DataEntrega = loteOrigem.DataEntrega,
                        NFe = loteOrigem.NFe,
                        DataValidade = loteOrigem.DataValidade,
                        DataHoraInsercaoRegistro = loteOrigem.DataHoraInsercaoRegistro
                    };
                    lotesParaAdicionar.Add(novoLote);
                    houveMudancas = true;
                }
            }

            // Adicionar novos lotes à coleção correta
            if (lotesParaAdicionar.Count > 0)
            {
                if (ehDestinoLocal)
                {
                    foreach (var l in lotesParaAdicionar) destino.ItensEstoque.Add(l);
                }
                else
                {
                    if (destino.ItensEstoqueDynamo == null) destino.ItensEstoqueDynamo = new List<ItemEstoqueModel>();
                    destino.ItensEstoqueDynamo.AddRange(lotesParaAdicionar);
                }
            }

            return houveMudancas;
        }

        // ==========================================================================================
        // 🔄 SINCRONIZAÇÃO DE INSUMOS
        // ==========================================================================================
        private async Task SincronizarInsumosAsync()
        {
            try
            {
                Console.WriteLine("=== SINCRONIZAÇÃO INSUMOS ===");
                var localItens = await _localDbContext.Insumos
                    .Include(i => i.ItemNivelEstoque).Include(i => i.ItensEstoque).ToListAsync();

                var dynamoItens = await _dynamoDBContext.ScanAsync<InsumosModel>(new List<ScanCondition>()).GetRemainingAsync();
                var dynamoMap = dynamoItens.ToDictionary(i => i.IdItem);
                var itemsParaEnviar = new List<InsumosModel>();

                foreach (var localItem in localItens)
                {
                    if (dynamoMap.TryGetValue(localItem.IdItem, out var dynamoItem))
                    {
                        // Tratar Exclusão
                        if (dynamoItem.IsDeleted && !localItem.IsDeleted)
                        {
                            localItem.IsDeleted = true;
                            dynamoMap.Remove(localItem.IdItem);
                            continue;
                        }

                        // 🛠️ CORREÇÃO BUG UTC (SQLite retorna Unspecified)
                        GarantirUtcLocal(localItem);

                        // 1. LOCAL É MAIS RECENTE -> Envia para Nuvem
                        if (localItem.DataAtualizacao > dynamoItem.DataAtualizacao)
                        {
                            Console.WriteLine($"   ⬆️ Local vence: {localItem.IdItem}");
                            // Garante que não perdemos lotes novos da nuvem mesmo se local venceu
                            MesclarLotes(localItem, dynamoItem, ehDestinoLocal: true);

                            SyncHelpers.PrepararParaDynamoDB(localItem);
                            itemsParaEnviar.Add(localItem);
                        }
                        // 2. NUVEM É MAIS RECENTE -> Atualiza Local
                        else if (dynamoItem.DataAtualizacao > localItem.DataAtualizacao)
                        {
                            Console.WriteLine($"   ⬇️ Nuvem vence: {localItem.IdItem}");

                            // Copiar escalares
                            localItem.CodInsumo = dynamoItem.CodInsumo;
                            localItem.DescricaoSimplificada = dynamoItem.DescricaoSimplificada;
                            localItem.DescricaoDetalhada = dynamoItem.DescricaoDetalhada;
                            localItem.Unidade = dynamoItem.Unidade;
                            localItem.IsDeleted = dynamoItem.IsDeleted;
                            localItem.DataAtualizacao = dynamoItem.DataAtualizacao; // Importante!

                            // Nivel Estoque
                            if (dynamoItem.ItemNivelEstoqueDynamo != null)
                            {
                                if (localItem.ItemNivelEstoque == null) localItem.ItemNivelEstoque = new ItemNivelEstoqueModel();
                                localItem.ItemNivelEstoque.NivelMinimoEstoque = dynamoItem.ItemNivelEstoqueDynamo.NivelMinimoEstoque;
                            }

                            // Mesclar Lotes (Nuvem -> Local)
                            bool mudouLotes = MesclarLotes(localItem, dynamoItem, ehDestinoLocal: true);

                            // Se a mesclagem alterou algo localmente que precisa voltar pra nuvem (raro, mas seguro)
                            if (mudouLotes)
                            {
                                SyncHelpers.PrepararParaDynamoDB(localItem);
                                itemsParaEnviar.Add(localItem);
                            }
                        }
                        else // DATAS IGUAIS
                        {
                            // Ainda assim verifica lotes
                            bool mudouLotes = MesclarLotes(localItem, dynamoItem, ehDestinoLocal: true);
                            if (mudouLotes)
                            {
                                SyncHelpers.PrepararParaDynamoDB(localItem);
                                itemsParaEnviar.Add(localItem);
                            }
                        }
                        dynamoMap.Remove(localItem.IdItem);
                    }
                    else
                    {
                        // NOVO NO LOCAL -> ENVIA
                        GarantirUtcLocal(localItem); // 🛠️ Correção UTC
                        SyncHelpers.PrepararParaDynamoDB(localItem);
                        itemsParaEnviar.Add(localItem);
                    }
                }

                // NOVO NO DYNAMO -> BAIXA
                foreach (var dynamoItem in dynamoMap.Values)
                {
                    if (!await _localDbContext.Insumos.AnyAsync(i => i.IdItem == dynamoItem.IdItem))
                    {
                        SyncHelpers.PrepararParaEFCore(dynamoItem); // Prepara navegações
                        await _localDbContext.Insumos.AddAsync(dynamoItem);
                    }
                }

                await EnviarLotesDynamo(itemsParaEnviar);
                await _localDbContext.SaveChangesAsync();
                Console.WriteLine("✅ Insumos Sincronizados");
            }
            catch (Exception ex) { Console.WriteLine($"❌ Erro Insumos: {ex.Message}"); throw; }
        }

        // ==========================================================================================
        // 🔄 SINCRONIZAÇÃO DE MEDICAMENTOS (Mesma Lógica)
        // ==========================================================================================
        private async Task SincronizarMedicamentosAsync()
        {
            try
            {
                Console.WriteLine("=== SINCRONIZAÇÃO MEDICAMENTOS ===");
                var localItens = await _localDbContext.Medicamentos
                    .Include(m => m.ItemNivelEstoque).Include(m => m.ItensEstoque).ToListAsync();

                var dynamoItens = await _dynamoDBContext.ScanAsync<MedicamentosModel>(new List<ScanCondition>()).GetRemainingAsync();
                var dynamoMap = dynamoItens.ToDictionary(i => i.IdItem);
                var itemsParaEnviar = new List<MedicamentosModel>();

                foreach (var localItem in localItens)
                {
                    if (dynamoMap.TryGetValue(localItem.IdItem, out var dynamoItem))
                    {
                        if (dynamoItem.IsDeleted && !localItem.IsDeleted)
                        {
                            localItem.IsDeleted = true; dynamoMap.Remove(localItem.IdItem); continue;
                        }

                        GarantirUtcLocal(localItem); // 🛠️ UTC Fix

                        if (localItem.DataAtualizacao > dynamoItem.DataAtualizacao)
                        {
                            Console.WriteLine($"   ⬆️ Local vence: {localItem.IdItem}");
                            MesclarLotes(localItem, dynamoItem, ehDestinoLocal: true);
                            SyncHelpers.PrepararParaDynamoDB(localItem);
                            itemsParaEnviar.Add(localItem);
                        }
                        else if (dynamoItem.DataAtualizacao > localItem.DataAtualizacao)
                        {
                            Console.WriteLine($"   ⬇️ Nuvem vence: {localItem.IdItem}");
                            // Atualizar Escalares
                            localItem.CodMedicamento = dynamoItem.CodMedicamento;
                            localItem.NomeComercial = dynamoItem.NomeComercial;
                            localItem.DescricaoMedicamento = dynamoItem.DescricaoMedicamento;
                            localItem.Formula = dynamoItem.Formula;
                            localItem.IsDeleted = dynamoItem.IsDeleted;
                            localItem.DataAtualizacao = dynamoItem.DataAtualizacao;

                            if (dynamoItem.ItemNivelEstoqueDynamo != null)
                            {
                                if (localItem.ItemNivelEstoque == null) localItem.ItemNivelEstoque = new ItemNivelEstoqueModel();
                                localItem.ItemNivelEstoque.NivelMinimoEstoque = dynamoItem.ItemNivelEstoqueDynamo.NivelMinimoEstoque;
                            }

                            bool mudou = MesclarLotes(localItem, dynamoItem, true);
                            if (mudou) { SyncHelpers.PrepararParaDynamoDB(localItem); itemsParaEnviar.Add(localItem); }
                        }
                        else
                        {
                            bool mudou = MesclarLotes(localItem, dynamoItem, true);
                            if (mudou) { SyncHelpers.PrepararParaDynamoDB(localItem); itemsParaEnviar.Add(localItem); }
                        }
                        dynamoMap.Remove(localItem.IdItem);
                    }
                    else
                    {
                        GarantirUtcLocal(localItem);
                        SyncHelpers.PrepararParaDynamoDB(localItem);
                        itemsParaEnviar.Add(localItem);
                    }
                }

                foreach (var dynamoItem in dynamoMap.Values)
                {
                    if (!await _localDbContext.Medicamentos.AnyAsync(m => m.IdItem == dynamoItem.IdItem))
                    {
                        SyncHelpers.PrepararParaEFCore(dynamoItem);
                        await _localDbContext.Medicamentos.AddAsync(dynamoItem);
                    }
                }

                await EnviarLotesDynamo(itemsParaEnviar);
                await _localDbContext.SaveChangesAsync();
                Console.WriteLine("✅ Medicamentos Sincronizados");
            }
            catch (Exception ex) { Console.WriteLine($"❌ Erro Medicamentos: {ex.Message}"); throw; }
        }

        // ==========================================================================================
        // 🔄 SINCRONIZAÇÃO DE PRODUTOS (Mesma Lógica)
        // ==========================================================================================
        private async Task SincronizarProdutosAsync()
        {
            try
            {
                Console.WriteLine("=== SINCRONIZAÇÃO PRODUTOS ===");
                var localItens = await _localDbContext.Produtos
                    .Include(p => p.ItemNivelEstoque).Include(p => p.ItensEstoque).ToListAsync();

                var dynamoItens = await _dynamoDBContext.ScanAsync<ProdutosModel>(new List<ScanCondition>()).GetRemainingAsync();
                var dynamoMap = dynamoItens.ToDictionary(i => i.IdItem);
                var itemsParaEnviar = new List<ProdutosModel>();

                foreach (var localItem in localItens)
                {
                    if (dynamoMap.TryGetValue(localItem.IdItem, out var dynamoItem))
                    {
                        if (dynamoItem.IsDeleted && !localItem.IsDeleted)
                        {
                            localItem.IsDeleted = true; dynamoMap.Remove(localItem.IdItem); continue;
                        }

                        GarantirUtcLocal(localItem); // 🛠️ UTC Fix

                        if (localItem.DataAtualizacao > dynamoItem.DataAtualizacao)
                        {
                            Console.WriteLine($"   ⬆️ Local vence: {localItem.IdItem}");
                            MesclarLotes(localItem, dynamoItem, true);
                            SyncHelpers.PrepararParaDynamoDB(localItem);
                            itemsParaEnviar.Add(localItem);
                        }
                        else if (dynamoItem.DataAtualizacao > localItem.DataAtualizacao)
                        {
                            Console.WriteLine($"   ⬇️ Nuvem vence: {localItem.IdItem}");
                            // Escalares
                            localItem.CodProduto = dynamoItem.CodProduto;
                            localItem.DescricaoSimples = dynamoItem.DescricaoSimples;
                            localItem.DescricaoDetalhada = dynamoItem.DescricaoDetalhada;
                            localItem.Categoria = dynamoItem.Categoria;
                            localItem.IsDeleted = dynamoItem.IsDeleted;
                            localItem.DataAtualizacao = dynamoItem.DataAtualizacao;

                            if (dynamoItem.ItemNivelEstoqueDynamo != null)
                            {
                                if (localItem.ItemNivelEstoque == null) localItem.ItemNivelEstoque = new ItemNivelEstoqueModel();
                                localItem.ItemNivelEstoque.NivelMinimoEstoque = dynamoItem.ItemNivelEstoqueDynamo.NivelMinimoEstoque;
                            }

                            bool mudou = MesclarLotes(localItem, dynamoItem, true);
                            if (mudou) { SyncHelpers.PrepararParaDynamoDB(localItem); itemsParaEnviar.Add(localItem); }
                        }
                        else
                        {
                            bool mudou = MesclarLotes(localItem, dynamoItem, true);
                            if (mudou) { SyncHelpers.PrepararParaDynamoDB(localItem); itemsParaEnviar.Add(localItem); }
                        }
                        dynamoMap.Remove(localItem.IdItem);
                    }
                    else
                    {
                        GarantirUtcLocal(localItem);
                        SyncHelpers.PrepararParaDynamoDB(localItem);
                        itemsParaEnviar.Add(localItem);
                    }
                }

                foreach (var dynamoItem in dynamoMap.Values)
                {
                    if (!await _localDbContext.Produtos.AnyAsync(p => p.IdItem == dynamoItem.IdItem))
                    {
                        SyncHelpers.PrepararParaEFCore(dynamoItem);
                        await _localDbContext.Produtos.AddAsync(dynamoItem);
                    }
                }

                await EnviarLotesDynamo(itemsParaEnviar);
                await _localDbContext.SaveChangesAsync();
                Console.WriteLine("✅ Produtos Sincronizados");
            }
            catch (Exception ex) { Console.WriteLine($"❌ Erro Produtos: {ex.Message}"); throw; }
        }

        // ==========================================================================================
        // AUXILIAR DE ENVIO LOTEADO (GENÉRICO)
        // ==========================================================================================
        private async Task EnviarLotesDynamo<T>(List<T> items) where T : class
        {
            if (items.Count == 0) return;
            Console.WriteLine($"📤 Enviando {items.Count} itens para DynamoDB...");
            var batchWriter = _dynamoDBContext.CreateBatchWrite<T>();

            for (int i = 0; i < items.Count; i++)
            {
                batchWriter.AddPutItem(items[i]);
                if ((i + 1) % 25 == 0 || (i + 1) == items.Count)
                {
                    await batchWriter.ExecuteAsync();
                    if ((i + 1) < items.Count) batchWriter = _dynamoDBContext.CreateBatchWrite<T>();
                }
            }
        }

        // Mantive o SincronizarRetiradaEstoqueAsync e os métodos de LimparExcluidos inalterados...
        // (Copie os métodos Limpar... e SincronizarRetirada... do seu código original, pois eles já estavam corretos)
        private async Task SincronizarRetiradaEstoqueAsync()
        {
            // ... (Manter o código de Retirada que você já tem, ele é mais simples e já estava OK)
            try
            {
                Console.WriteLine("=== SINCRONIZAÇÃO RETIRADA ESTOQUE ===");
                var localDbSet = _localDbContext.RetiradaEstoque;
                var localItens = await localDbSet.ToListAsync();
                var dynamoItens = await _dynamoDBContext.ScanAsync<RetiradaEstoqueModel>(new List<ScanCondition>()).GetRemainingAsync();
                var dynamoMap = dynamoItens.ToDictionary(i => i.IdRetirada);
                var itemsParaEnviar = new List<RetiradaEstoqueModel>();

                foreach (var localItem in localItens)
                {
                    if (dynamoMap.TryGetValue(localItem.IdRetirada, out var dynamoItem))
                    {
                        // Retirada não tem Lotes complexos, apenas comparação direta
                        if (localItem.DataAtualizacao > dynamoItem.DataAtualizacao)
                        {
                            itemsParaEnviar.Add(localItem);
                        }
                        else if (dynamoItem.DataAtualizacao > localItem.DataAtualizacao)
                        {
                            _localDbContext.Entry(localItem).CurrentValues.SetValues(dynamoItem);
                        }
                        dynamoMap.Remove(localItem.IdRetirada);
                    }
                    else
                    {
                        itemsParaEnviar.Add(localItem);
                    }
                }

                foreach (var dynamoItem in dynamoMap.Values)
                {
                    if (!await localDbSet.AnyAsync(r => r.IdRetirada == dynamoItem.IdRetirada))
                        await localDbSet.AddAsync(dynamoItem);
                }

                await EnviarLotesDynamo(itemsParaEnviar);
                await _localDbContext.SaveChangesAsync();
            }
            catch (Exception ex) { Console.WriteLine($"❌ Erro Retiradas: {ex.Message}"); throw; }
        }


        // Métodos de limpeza mantidos...
        private async Task LimparProdutosExcluidosAsync()
        {
            var itensParaDeletar = await _localDbContext.Produtos
                .Where(m => m.IsDeleted == true)
                .ToListAsync();

            if (itensParaDeletar.Count == 0) return;

            var dynamoBatchDeleter = _dynamoDBContext.CreateBatchWrite<ProdutosModel>();
            foreach (var item in itensParaDeletar)
            {
                dynamoBatchDeleter.AddDeleteItem(item);
            }

            _localDbContext.Produtos.RemoveRange(itensParaDeletar);
            await dynamoBatchDeleter.ExecuteAsync();
            await _localDbContext.SaveChangesAsync();
        }

        private async Task LimparMedicamentosExcluidosAsync()
        {
            var itensParaDeletar = await _localDbContext.Medicamentos
                .Where(m => m.IsDeleted == true)
                .ToListAsync();

            if (itensParaDeletar.Count == 0) return;

            var batchDeleter = _dynamoDBContext.CreateBatchWrite<MedicamentosModel>();
            foreach (var item in itensParaDeletar)
            {
                batchDeleter.AddDeleteItem(item);
            }

            _localDbContext.Medicamentos.RemoveRange(itensParaDeletar);
            await batchDeleter.ExecuteAsync();
            await _localDbContext.SaveChangesAsync();
        }

        private async Task LimparInsumosExcluidosAsync()
        {
            var itensParaDeletar = await _localDbContext.Insumos
                .Where(m => m.IsDeleted == true)
                .ToListAsync();

            if (itensParaDeletar.Count == 0) return;

            var batchDeleter = _dynamoDBContext.CreateBatchWrite<InsumosModel>();
            foreach (var item in itensParaDeletar)
            {
                batchDeleter.AddDeleteItem(item);
            }

            _localDbContext.Insumos.RemoveRange(itensParaDeletar);
            await batchDeleter.ExecuteAsync();
            await _localDbContext.SaveChangesAsync();
        }

        private async Task LimparRetiradaEstoqueExcluidosAsync()
        {
            try
            {
                Console.WriteLine("=== LIMPEZA RETIRADA ESTOQUE (SOFT DELETE) ===");

                var localDbSet = _localDbContext.RetiradaEstoque;

                var itensParaDeletar = await localDbSet
                    .Where(r => r.IsDeleted == true)
                    .ToListAsync();

                if (itensParaDeletar.Count == 0)
                {
                    Console.WriteLine("ℹ️  Nenhuma retirada marcada para exclusão");
                    return;
                }

                Console.WriteLine($"🗑️  {itensParaDeletar.Count} retiradas marcadas para exclusão");

                var dynamoBatchDeleter = _dynamoDBContext.CreateBatchWrite<RetiradaEstoqueModel>();
                foreach (var item in itensParaDeletar)
                {
                    dynamoBatchDeleter.AddDeleteItem(item);
                }

                await dynamoBatchDeleter.ExecuteAsync();
                Console.WriteLine("✅ Retiradas excluídas do DynamoDB");

                localDbSet.RemoveRange(itensParaDeletar);
                await _localDbContext.SaveChangesAsync();
                Console.WriteLine("✅ Retiradas excluídas do banco local");

                Console.WriteLine("=== LIMPEZA CONCLUÍDA ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro: {ex.Message}");
                throw;
            }
        }
    }
}
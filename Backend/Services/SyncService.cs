using Amazon.DynamoDBv2.DataModel;
using Backend.Context;
using Backend.Models.Medicamentos;
using Backend.Models.Produtos;
using Backend.Models.Insumos; 
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System; // Adicionado para Console.WriteLine
using System.Linq; // Adicionado para ToDictionary, etc.
using Backend.Services.Interfaces; // Adicionado para ISyncService
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
        }

        public async Task LimparRegistrosExcluidosAsync()
        {
            await SincronizarTabelasAsync();
            await LimparMedicamentosExcluidosAsync();
            await LimparProdutosExcluidosAsync();
            await LimparInsumosExcluidosAsync();
        }

        private async Task SincronizarMedicamentosAsync()
        {
            var localDbSet = _localDbContext.Medicamentos;

            var dynamoItens = await _dynamoDBContext.ScanAsync<MedicamentosModel>(new List<ScanCondition>()).GetRemainingAsync();
            var localItens = await localDbSet.AsNoTracking().ToListAsync();

            var dynamoMap = dynamoItens
                      .GroupBy(i => i.IdItem)
                      .ToDictionary(g => g.Key, g => g.First());

            var localMap = localItens
                    .GroupBy(i => i.IdItem)
                    .ToDictionary(g => g.Key, g => g.First());

            var dynamoBatchWriter = _dynamoDBContext.CreateBatchWrite<MedicamentosModel>();
            bool changesToLocalDb = false;

            foreach (var localItem in localMap.Values)
            {
                if (dynamoMap.TryGetValue(localItem.IdItem, out var dynamoItem))
                {
                    if (localItem.DataAtualizacao > dynamoItem.DataAtualizacao)
                    {
                        dynamoBatchWriter.AddPutItem(localItem);
                    }
                    else if (dynamoItem.DataAtualizacao > localItem.DataAtualizacao)
                    {
                        localDbSet.Update(dynamoItem);
                        changesToLocalDb = true;
                    }
                    dynamoMap.Remove(localItem.IdItem);
                }
                else
                {
                    dynamoBatchWriter.AddPutItem(localItem);
                }
            }

            foreach (var dynamoItem in dynamoMap.Values)
            {
                await localDbSet.AddAsync(dynamoItem);
                changesToLocalDb = true;
            }

            await dynamoBatchWriter.ExecuteAsync();
            if (changesToLocalDb)
            {
                await _localDbContext.SaveChangesAsync();
            }
        }

        private async Task SincronizarProdutosAsync()
        {
            try
            {
                Console.WriteLine("=== INÍCIO SINCRONIZAÇÃO PRODUTOS ===");
                var localDbSet = _localDbContext.Produtos;

                // PASSO 1: Verificar itens locais
                Console.WriteLine("📦 Carregando produtos locais...");
                var localItens = await localDbSet.AsNoTracking().ToListAsync();
                Console.WriteLine($"✅ {localItens.Count} produtos encontrados localmente");

                // Verificar IDs vazios
                var produtosSemId = localItens.Where(p => p.IdItem > 0).ToList();
                if (produtosSemId.Any())
                {
                    throw new InvalidOperationException(
                        $"❌ ERRO: {produtosSemId.Count} produtos com IdProduto vazio! " +
                        $"Primeiro: '{produtosSemId.First().DescricaoSimples}'");
                }

                if (localItens.Any())
                {
                    Console.WriteLine($"   Exemplo de ID local: '{localItens.First().IdItem}'");
                }

                // PASSO 2: Buscar do DynamoDB
                Console.WriteLine("☁️  Iniciando scan do DynamoDB...");
                var dynamoItens = await _dynamoDBContext
                    .ScanAsync<ProdutosModel>(new List<ScanCondition>())
                    .GetRemainingAsync();

                Console.WriteLine($"✅ {dynamoItens.Count} produtos encontrados no DynamoDB");

                if (dynamoItens.Any())
                {
                    var primeiroItem = dynamoItens.First();
                    Console.WriteLine($"   Exemplo de ID DynamoDB: '{primeiroItem.IdItem}'");
                    Console.WriteLine($"   Descrição: '{primeiroItem.DescricaoSimples}'");
                }

                // PASSO 3: Criar dicionários
                Console.WriteLine("🔄 Criando dicionários para comparação...");
                var dynamoMap = dynamoItens.ToDictionary(i => i.IdItem);

                var itemsParaEnviarAoDynamo = new List<ProdutosModel>();
                bool changesToLocalDb = false;

                // PASSO 4: Comparar e sincronizar
                Console.WriteLine("🔍 Comparando itens locais vs DynamoDB...");
                foreach (var localItem in localItens)
                {
                    if (dynamoMap.TryGetValue(localItem.IdItem, out var dynamoItem))
                    {
                        // Item existe em ambos
                        if (localItem.DataAtualizacao > dynamoItem.DataAtualizacao)
                        {
                            Console.WriteLine($"   ⬆️  Local mais novo: {localItem.IdItem}");
                            itemsParaEnviarAoDynamo.Add(localItem);
                        }
                        else if (dynamoItem.DataAtualizacao > localItem.DataAtualizacao)
                        {
                            Console.WriteLine($"   ⬇️  DynamoDB mais novo: {localItem.IdItem}");
                            localDbSet.Update(dynamoItem);
                            changesToLocalDb = true;
                        }
                        else
                        {
                            Console.WriteLine($"   ✔️  Igual: {localItem.IdItem}");
                        }
                        dynamoMap.Remove(localItem.IdItem);
                    }
                    else
                    {
                        // Item apenas local
                        Console.WriteLine($"   🆕 Novo no local: {localItem.IdItem}");
                        itemsParaEnviarAoDynamo.Add(localItem);
                    }
                }

                // PASSO 5: Itens apenas no DynamoDB
                foreach (var dynamoItem in dynamoMap.Values)
                {
                    Console.WriteLine($"   ☁️  Novo no DynamoDB: {dynamoItem.IdItem}");
                    await localDbSet.AddAsync(dynamoItem);
                    changesToLocalDb = true;
                }

                // PASSO 6: Enviar para DynamoDB em lotes
                if (itemsParaEnviarAoDynamo.Count > 0)
                {
                    Console.WriteLine($"📤 Enviando {itemsParaEnviarAoDynamo.Count} itens para DynamoDB...");
                    var dynamoBatchWriter = _dynamoDBContext.CreateBatchWrite<ProdutosModel>();

                    for (int i = 0; i < itemsParaEnviarAoDynamo.Count; i++)
                    {
                        dynamoBatchWriter.AddPutItem(itemsParaEnviarAoDynamo[i]);

                        if ((i + 1) % 25 == 0 || (i + 1) == itemsParaEnviarAoDynamo.Count)
                        {
                            Console.WriteLine($"   Enviando lote {(i / 25) + 1} ({Math.Min(i + 1, itemsParaEnviarAoDynamo.Count)} itens)...");
                            await dynamoBatchWriter.ExecuteAsync();

                            if ((i + 1) < itemsParaEnviarAoDynamo.Count)
                            {
                                dynamoBatchWriter = _dynamoDBContext.CreateBatchWrite<ProdutosModel>();
                            }
                        }
                    }
                    Console.WriteLine("✅ Envio para DynamoDB concluído!");
                }
                else
                {
                    Console.WriteLine("ℹ️  Nenhum item para enviar ao DynamoDB");
                }

                // PASSO 7: Salvar mudanças locais
                if (changesToLocalDb)
                {
                    Console.WriteLine("💾 Salvando mudanças no banco local...");
                    await _localDbContext.SaveChangesAsync();
                    Console.WriteLine("✅ Banco local atualizado!");
                }
                else
                {
                    Console.WriteLine("ℹ️  Nenhuma mudança no banco local");
                }

                Console.WriteLine("=== SINCRONIZAÇÃO PRODUTOS CONCLUÍDA ===");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"❌ ERRO DE OPERAÇÃO: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                throw;
            }
            catch (Amazon.DynamoDBv2.AmazonDynamoDBException ex)
            {
                Console.WriteLine($"❌ ERRO DYNAMODB: {ex.Message}");
                Console.WriteLine($"Código: {ex.ErrorCode}");
                Console.WriteLine($"Status: {ex.StatusCode}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO GERAL: {ex.Message}");
                Console.WriteLine($"Tipo: {ex.GetType().Name}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                throw;
            }
        }

        private async Task SincronizarInsumosAsync()
        {
            var localDbSet = _localDbContext.Insumos;

            var dynamoItens = await _dynamoDBContext.ScanAsync<InsumosModel>(new List<ScanCondition>()).GetRemainingAsync();
            var localItens = await localDbSet.AsNoTracking().ToListAsync();

            var dynamoMap = dynamoItens
                      .GroupBy(i => i.IdItem)
                      .ToDictionary(g => g.Key, g => g.First());

            var localMap = localItens
                    .GroupBy(i => i.IdItem)
                    .ToDictionary(g => g.Key, g => g.First());

            var dynamoBatchWriter = _dynamoDBContext.CreateBatchWrite<InsumosModel>();
            bool changesToLocalDb = false;

            foreach (var localItem in localItens)
            {
                if (dynamoMap.TryGetValue(localItem.IdItem, out var dynamoItem))
                {
                    if (localItem.DataAtualizacao > dynamoItem.DataAtualizacao)
                    {
                        dynamoBatchWriter.AddPutItem(localItem);
                    }
                    else if (dynamoItem.DataAtualizacao > localItem.DataAtualizacao)
                    {
                        localDbSet.Update(dynamoItem);
                        changesToLocalDb = true;
                    }
                    dynamoMap.Remove(localItem.IdItem);
                }
                else
                {
                    dynamoBatchWriter.AddPutItem(localItem);
                }
            }

            foreach (var dynamoItem in dynamoMap.Values)
            {
                await localDbSet.AddAsync(dynamoItem);
                changesToLocalDb = true;
            }

            // Precisamos lidar com lotes se a lista for grande
           
           
                await dynamoBatchWriter.ExecuteAsync();
            

            if (changesToLocalDb)
            {
                await _localDbContext.SaveChangesAsync();
            }
        }

        private async Task LimparMedicamentosExcluidosAsync()
        {
            var localDbSet = _localDbContext.Medicamentos;
            var itensParaDeletar = await localDbSet
                .Where(m => m.IsDeleted == true)
                .ToListAsync();

            if (itensParaDeletar.Count == 0) return;

            var dynamoBatchDeleter = _dynamoDBContext.CreateBatchWrite<MedicamentosModel>();
            foreach (var item in itensParaDeletar)
            {
                dynamoBatchDeleter.AddDeleteItem(item);
            }

            localDbSet.RemoveRange(itensParaDeletar);
            await dynamoBatchDeleter.ExecuteAsync();
            await _localDbContext.SaveChangesAsync();
        }

        private async Task LimparProdutosExcluidosAsync()
        {
            var localDbSet = _localDbContext.Produtos;

            var itensParaDeletar = await localDbSet
                .Where(m => m.IsDeleted == true)
                .ToListAsync();

            if (itensParaDeletar.Count == 0) return;

            var dynamoBatchDeleter = _dynamoDBContext.CreateBatchWrite<ProdutosModel>();
            foreach (var item in itensParaDeletar)
            {
                dynamoBatchDeleter.AddDeleteItem(item);
            }

            localDbSet.RemoveRange(itensParaDeletar);
            await dynamoBatchDeleter.ExecuteAsync();
            await _localDbContext.SaveChangesAsync();
        }

        private async Task LimparInsumosExcluidosAsync()
        {
            var localDbSet = _localDbContext.Insumos;

            var itensParaDeletar = await localDbSet
                .Where(m => m.IsDeleted == true)
                .ToListAsync();

            if (itensParaDeletar.Count == 0) return;

            var dynamoBatchDeleter = _dynamoDBContext.CreateBatchWrite<InsumosModel>();
            foreach (var item in itensParaDeletar)
            {
                dynamoBatchDeleter.AddDeleteItem(item);
            }

            localDbSet.RemoveRange(itensParaDeletar);
            await dynamoBatchDeleter.ExecuteAsync();
            await _localDbContext.SaveChangesAsync();
        }
    }
}
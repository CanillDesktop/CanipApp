using Backend.Models;
using Backend.Models.Produtos;
using Backend.Models.Medicamentos;
using Backend.Models.Insumos;

namespace Backend.Helpers
{
    public static class SyncHelpers
    {
        /// <summary>
        /// Sincroniza propriedades EF Core → DynamoDB antes de salvar no DynamoDB
        /// E corrige o Timezone (Kind=Unspecified para Kind=Utc)
        /// </summary>
        public static void PrepararParaDynamoDB<T>(T item) where T : ItemComEstoqueBaseModel
        {
            // 1. CORREÇÃO DE TIMEZONE: DATA DE INSERÇÃO (Base)
            item.DataHoraInsercaoRegistro = DateTime.SpecifyKind(item.DataHoraInsercaoRegistro, DateTimeKind.Utc);

            // 2. CORREÇÃO DE TIMEZONE: DATA DE ATUALIZAÇÃO (Específico por Tipo)
            // Como DataAtualizacao não está na base, verificamos o tipo do item
            if (item is InsumosModel insumo)
            {
                insumo.DataAtualizacao = DateTime.SpecifyKind(insumo.DataAtualizacao, DateTimeKind.Utc);
            }
            else if (item is MedicamentosModel med)
            {
                med.DataAtualizacao = DateTime.SpecifyKind(med.DataAtualizacao, DateTimeKind.Utc);
            }
            else if (item is ProdutosModel prod)
            {
                prod.DataAtualizacao = DateTime.SpecifyKind(prod.DataAtualizacao, DateTimeKind.Utc);
            }

            // 3. CORREÇÃO DE TIMEZONE: NÍVEL DE ESTOQUE (Filho)
            if (item.ItemNivelEstoque != null)
            {
                item.ItemNivelEstoque.DataHoraInsercaoRegistro =
                    DateTime.SpecifyKind(item.ItemNivelEstoque.DataHoraInsercaoRegistro, DateTimeKind.Utc);

                // Copiar para propriedade do Dynamo
                item.ItemNivelEstoqueDynamo = item.ItemNivelEstoque;
            }

            // 4. CORREÇÃO DE TIMEZONE: LISTA DE ESTOQUE (Filhos/Lotes)
            if (item.ItensEstoque != null)
            {
                foreach (var lote in item.ItensEstoque)
                {
                    // Data principal do lote
                    lote.DataHoraInsercaoRegistro = DateTime.SpecifyKind(lote.DataHoraInsercaoRegistro, DateTimeKind.Utc);

                    // Outras datas de negócio (Entrega/Validade)
                    lote.DataEntrega = DateTime.SpecifyKind(lote.DataEntrega, DateTimeKind.Utc);

                    if (lote.DataValidade.HasValue)
                    {
                        lote.DataValidade = DateTime.SpecifyKind(lote.DataValidade.Value, DateTimeKind.Utc);
                    }
                }

                // Copiar para propriedade do Dynamo
                item.ItensEstoqueDynamo = item.ItensEstoque.ToList();
            }
        }

        /// <summary>
        /// Sincroniza propriedades DynamoDB → EF Core após carregar do DynamoDB
        /// </summary>
        public static void PrepararParaEFCore<T>(T item) where T : ItemComEstoqueBaseModel
        {
            // Copiar propriedades DynamoDB para navegações EF Core
            if (item.ItemNivelEstoqueDynamo != null)
            {
                item.ItemNivelEstoque = item.ItemNivelEstoqueDynamo;
                item.ItemNivelEstoque.ItemBase = item;
            }

            if (item.ItensEstoqueDynamo != null && item.ItensEstoqueDynamo.Any())
            {
                // ✅ Verificar se já existe coleção rastreada pelo EF Core
                if (item.ItensEstoque == null || !item.ItensEstoque.Any())
                {
                    // ✅ Se não existe ou está vazia, podemos substituir
                    item.ItensEstoque = item.ItensEstoqueDynamo;
                }
                else
                {
                    // ✅ Se já existe, NÃO substituir para não perder o tracking do EF
                    Console.WriteLine($"   ⚠️  Item {item.IdItem}: ItensEstoque já existe, pulando sobrescrita para manter tracking");
                }

                // Restaurar navegações (Foreign Keys virtuais)
                foreach (var estoque in item.ItensEstoque ?? new List<ItemEstoqueModel>())
                {
                    estoque.ItemBase = item;
                }
            }
        }
    }
}


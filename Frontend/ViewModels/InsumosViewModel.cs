using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Models;
using Frontend.Models.Insumos;
using Frontend.Records;
using Frontend.ViewModels.Interfaces;
using Shared.DTOs.Insumos;
using Shared.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Json;

namespace Frontend.ViewModels
{
    public partial class InsumosViewModel : ObservableObject, ILoadableViewModel, ITabableViewModel
    {
        private readonly HttpClient _http;

        private bool _hasTabs;
        private readonly ObservableCollection<TabItemModel> _tabsShowing =
        [
            new TabItemModel("Insumos", true),
            new TabItemModel("Cadastrar")
        ];
        private string _activeTab = "Insumos";

        private bool _carregando;
        private bool _cadastrando;
        private bool _deletando;
        private bool _sincronizando; // ← NOVO

        private InsumosModel _insumoCadastro = new();

        private InsumosFiltroModel _filtro = new();
        private string _chavePesquisa = string.Empty;
        private string _valorPesquisa = string.Empty;

        public ObservableCollection<InsumosLeituraDTO> Insumos { get; } = [];

        public bool Carregando
        {
            get => _carregando;
            set => SetProperty(ref _carregando, value);
        }

        public bool Cadastrando
        {
            get => _cadastrando;
            set => SetProperty(ref _cadastrando, value);
        }

        public bool Deletando
        {
            get => _deletando;
            set => SetProperty(ref _deletando, value);
        }

        // ← NOVA PROPRIEDADE
        public bool Sincronizando
        {
            get => _sincronizando;
            set => SetProperty(ref _sincronizando, value);
        }

        public bool HasTabs
        {
            get => _hasTabs;
            set => SetProperty(ref _hasTabs, value);
        }

        public ObservableCollection<TabItemModel> TabsShowing => _tabsShowing;

        public string ActiveTab
        {
            get => _activeTab;
            set
            {
                if (SetProperty(ref _activeTab, value))
                {
                    OnTabChanged?.Invoke();
                }
            }
        }

        public InsumosModel InsumoCadastro
        {
            get => _insumoCadastro;
            set => SetProperty(ref _insumoCadastro, value);
        }

        public InsumosFiltroModel Filtro
        {
            get => _filtro;
            set => SetProperty(ref _filtro, value);
        }

        public string ValorPesquisa
        {
            get => _valorPesquisa;
            set => SetProperty(ref _valorPesquisa, value);
        }

        public string ChavePesquisa
        {
            get => _chavePesquisa;
            set => SetProperty(ref _chavePesquisa, value);
        }

        public Action? OnTabChanged { get; set; }
        public Action? OnInitialLoad { get; set; }

        public IAsyncRelayCommand CarregarInsumosCommand;
        public IAsyncRelayCommand<InsumosModel?> CadastrarInsumoCommand;
        public IAsyncRelayCommand<PesquisaProduto?> FiltrarInsumosCommand;
        public IAsyncRelayCommand<InsumosLeituraDTO?> DeletarInsumoCommand;
        public IAsyncRelayCommand SincronizarInsumosCommand; // ← NOVO COMANDO

        public InsumosViewModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
            CarregarInsumosCommand = new AsyncRelayCommand(CarregarInsumosAsync);
            CadastrarInsumoCommand = new AsyncRelayCommand<InsumosModel?>(CadastrarInsumoAsync);
            FiltrarInsumosCommand = new AsyncRelayCommand<PesquisaProduto?>(BuscarInsumosFiltradosAsync);
            DeletarInsumoCommand = new AsyncRelayCommand<InsumosLeituraDTO?>(DeletarInsumoAsync);
            SincronizarInsumosCommand = new AsyncRelayCommand(SincronizarInsumosAsync); // ← NOVO
        }

        #region metodos
        private async Task CarregarInsumosAsync()
        {
            try
            {
                if (Carregando)
                    return;

                Debug.WriteLine("[InsumosViewModel] 📥 Iniciando carregamento de insumos...");
                Carregando = true;
                var insumos = await _http.GetFromJsonAsync<InsumosLeituraDTO[]>("api/insumos");

                Insumos.Clear();
                foreach (var i in insumos ?? [])
                    Insumos.Add(i);

                Debug.WriteLine($"[InsumosViewModel] ✅ {Insumos.Count} insumos carregados");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InsumosViewModel] ❌ Erro ao carregar insumos: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                Carregando = false;
            }
        }

        // ════════════════════════════════════════════════════════════
        // ← MÉTODO DE SINCRONIZAÇÃO (OPÇÃO 1: Chama SincronizarTabelasAsync)
        // ════════════════════════════════════════════════════════════
        private async Task SincronizarInsumosAsync()
        {
            try
            {
                if (Sincronizando)
                {
                    Debug.WriteLine("[InsumosViewModel] ⚠️  Sincronização já em andamento, ignorando...");
                    return;
                }

                Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Debug.WriteLine("[InsumosViewModel] 🔄 INICIANDO SINCRONIZAÇÃO COM AWS");
                Debug.WriteLine("[InsumosViewModel] ℹ️  Chamando SincronizarTabelasAsync() que sincroniza TODAS as tabelas");
                Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                Sincronizando = true;

                // ← CHAMA O MÉTODO QUE SINCRONIZA TODAS AS TABELAS
                // (Internamente chama SincronizarInsumosAsync, SincronizarMedicamentosAsync, etc)
                Debug.WriteLine("[InsumosViewModel] 📤 Enviando POST /api/sync");

                var response = await _http.PostAsync("api/sync", null);

                Debug.WriteLine($"[InsumosViewModel] 📥 Resposta recebida: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<SyncResponse>();

                    Debug.WriteLine($"[InsumosViewModel] ✅ Sincronização concluída: {resultado?.message}");

                    await Application.Current!.MainPage!.DisplayAlert(
                        "Sucesso",
                        "Sincronização com AWS concluída com sucesso!\n\nTodas as tabelas foram sincronizadas:\n• Insumos\n• Medicamentos\n• Produtos\n• Retirada Estoque",
                        "OK");

                    // Recarregar os dados após sincronização
                    await CarregarInsumosAsync();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[InsumosViewModel] ❌ Erro na sincronização: {error}");

                    await Application.Current!.MainPage!.DisplayAlert(
                        "Erro na Sincronização",
                        $"Erro ao sincronizar: {error}",
                        "OK");
                }

                Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"[InsumosViewModel] ❌ Erro HTTP: {httpEx.Message}");
                Debug.WriteLine($"[InsumosViewModel] StatusCode: {httpEx.StatusCode}");

                await Application.Current!.MainPage!.DisplayAlert(
                    "Erro de Conexão",
                    $"Não foi possível conectar ao servidor: {httpEx.Message}",
                    "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InsumosViewModel] ❌ Erro geral: {ex.Message}");
                Debug.WriteLine($"[InsumosViewModel] StackTrace: {ex.StackTrace}");

                await Application.Current!.MainPage!.DisplayAlert(
                    "Erro",
                    ex.Message,
                    "OK");
            }
            finally
            {
                Sincronizando = false;
                Debug.WriteLine("[InsumosViewModel] 🏁 Processo de sincronização finalizado");
            }
        }

        public async Task OnLoadedAsync()
        {
            await CarregarInsumosAsync();
            OnInitialLoad?.Invoke();
        }

        public void AbreAbaCadastro()
        {
            HasTabs = true;
            TabsShowing.First(t => t.Name == "Cadastrar").IsVisible = true;
            ActiveTab = TabsShowing.First(t => t.Name == "Cadastrar").Name;
        }

        private async Task CadastrarInsumoAsync(InsumosModel? insumo)
        {
            try
            {
                if (Cadastrando)
                    return;

                Debug.WriteLine("[InsumosViewModel] 📝 Cadastrando novo insumo...");

                var dto = (InsumosCadastroDTO)insumo;

                Cadastrando = true;
                var response = await _http.PostAsJsonAsync("api/insumos", dto);

                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("[InsumosViewModel] ✅ Insumo cadastrado com sucesso");
                    await Application.Current!.MainPage!.DisplayAlert("Sucesso", "Insumo cadastrado com sucesso!", "OK");

                    // Recarregar lista
                    await CarregarInsumosAsync();
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    Debug.WriteLine($"[InsumosViewModel] ❌ Erro no cadastro: {error?.Message}");
                    await Application.Current!.MainPage!.DisplayAlert(error!.Title, error!.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InsumosViewModel] ❌ Erro ao cadastrar: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                InsumoCadastro = new();
                Cadastrando = false;
            }
        }

        private async Task BuscarInsumosFiltradosAsync(PesquisaProduto? pesquisa)
        {
            try
            {
                if (Carregando)
                    return;

                Carregando = true;

                if (pesquisa == null)
                    return;

                if (string.IsNullOrWhiteSpace(ChavePesquisa) || string.IsNullOrWhiteSpace(ValorPesquisa))
                {
                    await Application.Current!.MainPage!.DisplayAlert("Aviso", "Escolha um campo e preencha o valor.", "OK");
                    return;
                }

                Debug.WriteLine($"[InsumosViewModel] 🔍 Filtrando por {ChavePesquisa}={ValorPesquisa}");

                var url = $"api/insumos?{ChavePesquisa}={Uri.UnescapeDataString(ValorPesquisa)}";
                var insumos = await _http.GetFromJsonAsync<InsumosLeituraDTO[]>(url);

                Insumos.Clear();
                foreach (var i in insumos ?? [])
                    Insumos.Add(i);

                Debug.WriteLine($"[InsumosViewModel] ✅ {Insumos.Count} insumos encontrados");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InsumosViewModel] ❌ Erro ao filtrar: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                Carregando = false;
            }
        }

        private async Task DeletarInsumoAsync(InsumosLeituraDTO? m)
        {
            try
            {
                if (Deletando)
                    return;

                Deletando = true;

                var isExcluir = await Application.Current!.MainPage!.DisplayAlert(
                    "Confirmação de exclusão",
                    $"Deseja realmente excluir o insumo \"{m.NomeItem}\"?",
                    "Sim",
                    "Não");

                if (isExcluir)
                {
                    Debug.WriteLine($"[InsumosViewModel] 🗑️  Deletando insumo ID: {m.IdItem}");

                    var result = await _http.DeleteAsync($"api/insumos/{m.IdItem}");

                    Debug.WriteLine($"[InsumosViewModel] ✅ Insumo deletado: {result.StatusCode}");

                    await CarregarInsumosAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InsumosViewModel] ❌ Erro ao deletar: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                Deletando = false;
            }
        }

        public static string DisplayDataEntregaRecente(InsumosLeituraDTO i) =>
            i.ItensEstoque.Length > 0 ?
            i.ItensEstoque?
            .Select(i => i.DataEntrega)?
            .OrderDescending()?
            .FirstOrDefault()
            .ToShortDateString() ?? "-"
            : "-";

        public static string DisplayDescricaoDetalhada(InsumosLeituraDTO i) =>
            string.IsNullOrEmpty(i.DescricaoDetalhada) ? "-" : i.DescricaoDetalhada;

        public static string DisplayDataValidadeRecente(InsumosLeituraDTO i) =>
            i.ItensEstoque?
            .Select(i => i.DataValidade)?
            .OrderDescending()?
            .FirstOrDefault()?
            .ToShortDateString() ?? "-";
        #endregion
    }

    // ← CLASSE AUXILIAR PARA DESERIALIZAÇÃO DA RESPOSTA
    public class SyncResponse
    {
        public string message { get; set; }
    }
}
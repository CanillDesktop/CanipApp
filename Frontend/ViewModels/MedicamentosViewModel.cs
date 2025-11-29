using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Models;
using Frontend.Models.Medicamentos;
using Frontend.Records;
using Frontend.ViewModels.Interfaces;
using Shared.DTOs.Medicamentos;
using Shared.DTOs.Produtos;
using Shared.Enums;
using Shared.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Json;

public record PesquisaMedicamento(string Chave, string Valor);

namespace Frontend.ViewModels
{
    public partial class MedicamentosViewModel : ObservableObject, ILoadableViewModel, ITabableViewModel
    {
        private readonly HttpClient _http;

        private bool _hasTabs;
        private readonly ObservableCollection<TabItemModel> _tabsShowing =
        [
            new TabItemModel("Medicamentos", true),
            new TabItemModel("Cadastrar")
        ];
        private string _activeTab = "Medicamentos";
        private bool _carregando;
        private bool _cadastrando;
        private bool _sincronizando;
        private bool _deletando;

        private MedicamentosModel _medicamentoCadastro = new();
        private MedicamentosFiltroModel _filtro = new();

        private string _chavePesquisa = string.Empty;
        private string _valorPesquisa = string.Empty;
        private string? _mensagemSucesso;
        private string? _mensagemErro;

        public ObservableCollection<MedicamentoLeituraDTO> Medicamentos { get; } = [];

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

        public bool Sincronizando
        {
            get => _sincronizando;
            set => SetProperty(ref _sincronizando, value);
        }

        public bool Deletando
        {
            get => _deletando;
            set => SetProperty(ref _deletando, value);
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

        public MedicamentosModel MedicamentoCadastro
        {
            get => _medicamentoCadastro;
            set => SetProperty(ref _medicamentoCadastro, value);
        }

        public MedicamentosFiltroModel Filtro
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

        public string? MensagemSucesso
        {
            get => _mensagemSucesso;
            set => SetProperty(ref _mensagemSucesso, value);
        }

        public string? MensagemErro
        {
            get => _mensagemErro;
            set => SetProperty(ref _mensagemErro, value);
        }

        public Action? OnTabChanged { get; set; }
        public Action? OnInitialLoad { get; set; }

        public IAsyncRelayCommand CarregarMedicamentosCommand { get; }
        public IAsyncRelayCommand<MedicamentosModel?> CadastrarMedicamentoCommand { get; }
        public IAsyncRelayCommand<PesquisaProduto?> FiltrarMedicamentosCommand { get; }
        public IAsyncRelayCommand<MedicamentoLeituraDTO?> DeletarMedicamentoCommand { get; }
        public IAsyncRelayCommand SincronizarMedicamentoCommand { get; }

        public MedicamentosViewModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");

            CarregarMedicamentosCommand = new AsyncRelayCommand(CarregarMedicamentosAsync);
            CadastrarMedicamentoCommand = new AsyncRelayCommand<MedicamentosModel?>(CadastrarMedicamentoAsync);
            FiltrarMedicamentosCommand = new AsyncRelayCommand<PesquisaProduto?>(BuscarMedicamentosFiltradosAsync);
            DeletarMedicamentoCommand = new AsyncRelayCommand<MedicamentoLeituraDTO?>(DeletarMedicamentoAsync);
            SincronizarMedicamentoCommand = new AsyncRelayCommand(SincronizarMedicamentoAsyncFront);
        }

        #region Metodos

        private async Task CarregarMedicamentosAsync()
        {
            try
            {
                if (Carregando) return;

                Carregando = true;
                LimparMensagens();

                // ============================================================================
                // 🔥 DEBUG: VALIDAR AUTENTICAÇÃO ANTES DE FAZER REQUEST
                // ============================================================================
                var idToken = await SecureStorage.GetAsync("id_token");
                Console.WriteLine($"🔍 [MedicamentosVM] CarregarMedicamentosAsync iniciado");
                Console.WriteLine($"🔑 [MedicamentosVM] ID Token existe? {!string.IsNullOrWhiteSpace(idToken)}");

                if (!string.IsNullOrWhiteSpace(idToken))
                {
                    Console.WriteLine($"🔑 [MedicamentosVM] Token preview: {idToken.Substring(0, Math.Min(50, idToken.Length))}...");
                }
                else
                {
                    Console.WriteLine($"❌ [MedicamentosVM] ERRO: Usuário não autenticado!");
                    MensagemErro = "❌ Você precisa estar logado para visualizar medicamentos";
                    return;
                }

                Console.WriteLine($"🌐 [MedicamentosVM] BaseAddress: {_http.BaseAddress}");
                Console.WriteLine($"📡 [MedicamentosVM] Fazendo GET /api/medicamentos...");

                var medicamentos = await _http.GetFromJsonAsync<MedicamentoLeituraDTO[]>("api/medicamentos");

                Console.WriteLine($"✅ [MedicamentosVM] Resposta recebida: {medicamentos?.Length ?? 0} medicamentos");

                Medicamentos.Clear();

                if (medicamentos != null && medicamentos.Length > 0)
                {
                    foreach (var m in medicamentos)
                    {
                        Medicamentos.Add(m);
                        Console.WriteLine($"   - {m.NomeItem} (ID: {m.IdItem})");
                    }

                    MensagemSucesso = $"✅ {medicamentos.Length} medicamento(s) carregado(s)";
                }
                else
                {
                    Console.WriteLine($"⚠️ [MedicamentosVM] Nenhum medicamento retornado");
                    MensagemSucesso = "ℹ️ Nenhum medicamento cadastrado";
                }
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"❌ Erro de conexão: {ex.Message}";
                Console.WriteLine($"❌ [MedicamentosVM] HttpRequestException: {ex.Message}");
                Console.WriteLine($"❌ [MedicamentosVM] StatusCode: {ex.StatusCode}");
                Console.WriteLine($"❌ [MedicamentosVM] Stack: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                MensagemErro = $"❌ Erro ao carregar medicamentos: {ex.Message}";
                Console.WriteLine($"❌ [MedicamentosVM] Exception: {ex.GetType().Name}");
                Console.WriteLine($"❌ [MedicamentosVM] Message: {ex.Message}");
                Console.WriteLine($"❌ [MedicamentosVM] Stack: {ex.StackTrace}");

                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                Carregando = false;
                Console.WriteLine($"🏁 [MedicamentosVM] CarregarMedicamentosAsync finalizado");
            }
        }

        public async Task OnLoadedAsync()
        {
            Console.WriteLine($"🔄 [MedicamentosVM] OnLoadedAsync chamado");
            await CarregarMedicamentosAsync();
            OnInitialLoad?.Invoke();
        }

        public void AbreAbaCadastro()
        {
            HasTabs = true;
            var tab = TabsShowing.FirstOrDefault(t => t.Name == "Cadastrar");
            if (tab != null)
            {
                tab.IsVisible = true;
                ActiveTab = tab.Name;
            }
            LimparMensagens();
        }

        private async Task CadastrarMedicamentoAsync(MedicamentosModel? med)
        {
            if (med == null) return;

            try
            {
                if (Cadastrando) return;

                LimparMensagens();
                var dto = (MedicamentoCadastroDTO)med;

                Cadastrando = true;

                Console.WriteLine($"📤 [MedicamentosVM] Cadastrando medicamento: {dto.NomeComercial}");

                var response = await _http.PostAsJsonAsync("api/medicamentos", dto);

                Console.WriteLine($"📡 [MedicamentosVM] Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    MensagemSucesso = "✅ Medicamento cadastrado com sucesso!";
                    Console.WriteLine("✅ [MedicamentosVM] Medicamento cadastrado com sucesso!");

                    await Task.Delay(2000);
                    await CarregarMedicamentosAsync();
                    OnTabChanged?.Invoke();

                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Sucesso", "Medicamento cadastrado com sucesso!", "OK");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MensagemErro = $"❌ Erro ao cadastrar: {errorContent}";
                    Console.WriteLine($"❌ [MedicamentosVM] Erro: {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"❌ Erro de conexão: {ex.Message}";
                Console.WriteLine($"❌ [MedicamentosVM] HttpRequestException ao cadastrar: {ex.Message}");

                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Erro de Conexão", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                MensagemErro = $"❌ Erro ao cadastrar medicamento: {ex.Message}";
                Console.WriteLine($"❌ [MedicamentosVM] Exception ao cadastrar: {ex.Message}");

                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                MedicamentoCadastro = new();
                Cadastrando = false;
            }
        }

        private async Task BuscarMedicamentosFiltradosAsync(PesquisaProduto? pesquisa)
        {
            try
            {
                if (Carregando) return;

                Carregando = true;
                LimparMensagens();

                if (string.IsNullOrWhiteSpace(ChavePesquisa) || string.IsNullOrWhiteSpace(ValorPesquisa))
                {
                    MensagemErro = "Escolha um campo e preencha o valor para pesquisar.";
                    Console.WriteLine("⚠️ [MedicamentosVM] Filtro inválido");

                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Aviso", "Escolha um campo e preencha o valor.", "OK");

                    return;
                }

                var url = $"api/medicamentos?{ChavePesquisa}={Uri.EscapeDataString(ValorPesquisa)}";
                Console.WriteLine($"🔍 [MedicamentosVM] Filtrando: {url}");

                var medicamentos = await _http.GetFromJsonAsync<MedicamentoLeituraDTO[]>(url);

                Console.WriteLine($"✅ [MedicamentosVM] Filtro retornou: {medicamentos?.Length ?? 0} resultados");

                Medicamentos.Clear();
                foreach (var p in medicamentos ?? [])
                    Medicamentos.Add(p);

                if (medicamentos?.Length == 0)
                {
                    MensagemSucesso = "Nenhum medicamento encontrado com os critérios informados.";
                }
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de conexão: {ex.Message}";
                Console.WriteLine($"❌ [MedicamentosVM] Erro ao filtrar: {ex.Message}");
            }
            catch (Exception ex)
            {
                MensagemErro = $"Erro ao filtrar: {ex.Message}";
                Console.WriteLine($"❌ [MedicamentosVM] Exception ao filtrar: {ex.Message}");

                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                Carregando = false;
            }
        }

        private async Task DeletarMedicamentoAsync(MedicamentoLeituraDTO? m)
        {
            if (m == null || Carregando) return;

            Carregando = true;
            LimparMensagens();

            try
            {
                if (Deletando) return;
                Deletando = true;

                bool isExcluir = false;
                if (Application.Current?.MainPage != null)
                {
                    isExcluir = await Application.Current.MainPage.DisplayAlert(
                        "Confirmação de exclusão",
                        $"Deseja realmente excluir o medicamento \"{m.NomeItem}\"?",
                        "Sim", "Não");
                }

                if (isExcluir)
                {
                    Console.WriteLine($"🗑️ [MedicamentosVM] Deletando ID: {m.IdItem}");
                    var result = await _http.DeleteAsync($"api/medicamentos/{m.IdItem}");

                    Console.WriteLine($"📡 [MedicamentosVM] Delete response: {result.StatusCode}");

                    if (result.IsSuccessStatusCode)
                    {
                        MensagemSucesso = "✅ Medicamento excluído com sucesso!";
                        await CarregarMedicamentosAsync();
                    }
                    else
                    {
                        MensagemErro = $"Erro ao deletar: {result.ReasonPhrase}";
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"❌ Erro de conexão: {ex.Message}";
                Console.WriteLine($"❌ [MedicamentosVM] Erro ao deletar: {ex.Message}");
            }
            catch (Exception ex)
            {
                MensagemErro = $"❌ Erro ao deletar: {ex.Message}";
                Console.WriteLine($"❌ [MedicamentosVM] Exception ao deletar: {ex.Message}");

                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                Deletando = false;
                Carregando = false;
            }
        }

        private async Task SincronizarMedicamentoAsyncFront()
        {
            try
            {
                if (Sincronizando) return;

                Sincronizando = true;
                LimparMensagens();

                Console.WriteLine($"🔄 [MedicamentosVM] Iniciando sincronização...");

                var response = await _http.PostAsync("api/Sync", null);

                Console.WriteLine($"📡 [MedicamentosVM] Sync response: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    MensagemSucesso = result != null && result.ContainsKey("message")
                        ? result["message"]
                        : "✅ Sincronização concluída com sucesso!";

                    await CarregarMedicamentosAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MensagemErro = $"❌ Erro na sincronização: {errorContent}";
                }
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"❌ Erro de conexão com servidor: {ex.Message}";
                Console.WriteLine($"❌ [MedicamentosVM] Erro ao sincronizar: {ex.Message}");
            }
            catch (Exception ex)
            {
                MensagemErro = $"❌ Erro ao sincronizar: {ex.Message}";
                Console.WriteLine($"❌ [MedicamentosVM] Exception ao sincronizar: {ex.Message}");
            }
            finally
            {
                Sincronizando = false;
            }
        }

        private void LimparMensagens()
        {
            MensagemSucesso = null;
            MensagemErro = null;
        }

        public static string DisplayDataEntregaRecente(MedicamentoLeituraDTO m)
        {
            if (m.ItensEstoque == null || m.ItensEstoque.Length == 0) return "-";

            var item = m.ItensEstoque
                .OrderByDescending(i => i.DataEntrega)
                .FirstOrDefault();

            return item != null ? item.DataEntrega.ToShortDateString() : "-";
        }

        public static string DisplayDataValidadeRecente(MedicamentoLeituraDTO m)
        {
            if (m.ItensEstoque == null || m.ItensEstoque.Length == 0) return "-";

            var item = m.ItensEstoque
                .OrderByDescending(i => i.DataValidade)
                .FirstOrDefault();

            return item != null && item.DataValidade.HasValue ? item.DataValidade.Value.ToShortDateString() : "-";
        }

        #endregion
    }
}
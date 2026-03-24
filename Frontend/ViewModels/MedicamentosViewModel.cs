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
            SincronizarMedicamentoCommand = new AsyncRelayCommand(SincronizarMedicamentosAsync);
        }

        #region Metodos

        private async Task CarregarMedicamentosAsync()
        {
            try
            {
                if (Carregando) return;

                Carregando = true;
                LimparMensagens();

                var idToken = await SecureStorage.GetAsync("id_token");
                if (string.IsNullOrWhiteSpace(idToken))
                {
                    MensagemErro = "❌ Você precisa estar logado para visualizar medicamentos";
                    return;
                }

                var medicamentos = await _http.GetFromJsonAsync<MedicamentoLeituraDTO[]>("api/medicamentos");

                Medicamentos.Clear();

                if (medicamentos != null && medicamentos.Length > 0)
                {
                    foreach (var m in medicamentos)
                        Medicamentos.Add(m);

                    MensagemSucesso = $"✅ {medicamentos.Length} medicamento(s) carregado(s)";
                }
                else
                {
                    MensagemSucesso = "ℹ️ Nenhum medicamento cadastrado";
                }
            }
            catch (Exception ex)
            {
                MensagemErro = $"❌ Erro ao carregar medicamentos: {ex.Message}";
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                Carregando = false;
            }
        }

        public async Task OnLoadedAsync()
        {
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

                var response = await _http.PostAsJsonAsync("api/medicamentos", dto);

                if (response.IsSuccessStatusCode)
                {
                    MensagemSucesso = "✅ Medicamento cadastrado com sucesso!";
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
                }
            }
            catch (Exception ex)
            {
                MensagemErro = $"❌ Erro ao cadastrar medicamento: {ex.Message}";
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
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Aviso", "Escolha um campo e preencha o valor.", "OK");
                    return;
                }

                var url = $"api/medicamentos?{ChavePesquisa}={Uri.EscapeDataString(ValorPesquisa)}";
                var medicamentos = await _http.GetFromJsonAsync<MedicamentoLeituraDTO[]>(url);

                Medicamentos.Clear();
                foreach (var p in medicamentos ?? [])
                    Medicamentos.Add(p);

                if (medicamentos?.Length == 0)
                {
                    MensagemSucesso = "Nenhum medicamento encontrado com os critérios informados.";
                }
            }
            catch (Exception ex)
            {
                MensagemErro = $"Erro ao filtrar: {ex.Message}";
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

                bool isExcluir = await Application.Current!.MainPage!.DisplayAlert(
                        "Confirmação de exclusão",
                        $"Deseja realmente excluir o medicamento \"{m.NomeItem}\"?",
                        "Sim", "Não");

                if (isExcluir)
                {
                    var result = await _http.DeleteAsync($"api/medicamentos/{m.IdItem}");

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
            catch (Exception ex)
            {
                MensagemErro = $"❌ Erro ao deletar: {ex.Message}";
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                Deletando = false;
                Carregando = false;
            }
        }

        // ════════════════════════════════════════════════════════════
        // ← MÉTODO DE SINCRONIZAÇÃO (Padronizado)
        // ════════════════════════════════════════════════════════════
        private async Task SincronizarMedicamentosAsync()
        {
            try
            {
                if (Sincronizando) return;

                Sincronizando = true;
                LimparMensagens();

                Debug.WriteLine("[MedicamentosViewModel] 🔄 INICIANDO SINCRONIZAÇÃO COM AWS");

                var response = await _http.PostAsync("api/sync", null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<SyncResponse>();
                    MensagemSucesso = result != null && !string.IsNullOrEmpty(result.message)
                        ? result.message
                        : "✅ Sincronização concluída com sucesso!";

                    await Application.Current!.MainPage!.DisplayAlert(
                        "Sucesso",
                        "Sincronização com AWS concluída com sucesso!\n\nTodas as tabelas foram atualizadas.",
                        "OK");

                    await CarregarMedicamentosAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MensagemErro = $"❌ Erro na sincronização: {errorContent}";
                    await Application.Current!.MainPage!.DisplayAlert("Erro na Sincronização", MensagemErro, "OK");
                }
            }
            catch (Exception ex)
            {
                MensagemErro = $"❌ Erro ao sincronizar: {ex.Message}";
                Debug.WriteLine($"❌ [MedicamentosViewModel] Erro: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
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
            var item = m.ItensEstoque.OrderByDescending(i => i.DataEntrega).FirstOrDefault();
            return item != null ? item.DataEntrega.ToShortDateString() : "-";
        }

        public static string DisplayDataValidadeRecente(MedicamentoLeituraDTO m)
        {
            if (m.ItensEstoque == null || m.ItensEstoque.Length == 0) return "-";
            var item = m.ItensEstoque.OrderByDescending(i => i.DataValidade).FirstOrDefault();
            return item != null && item.DataValidade.HasValue ? item.DataValidade.Value.ToShortDateString() : "-";
        }

        #endregion
    }
}
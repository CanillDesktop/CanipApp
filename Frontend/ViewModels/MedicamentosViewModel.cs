using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Models;
using Frontend.ViewModels.Interfaces;
using Shared.DTOs;
using Shared.Models;
using Frontend.Records;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

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
        private MedicamentosModel _medicamento = new();
        private MedicamentosFiltroModel _filtro = new();
        private string _chavePesquisa = string.Empty;
        private string _valorPesquisa = string.Empty;

        public ObservableCollection<MedicamentoDTO> Medicamentos { get; } = [];

        public bool Carregando
        {
            get => _carregando;
            set
            {
                SetProperty(ref _carregando, value);
            }
        }

        public bool Cadastrando
        {
            get => _cadastrando;
            set
            {
                SetProperty(ref _cadastrando, value);
            }
        }

        public bool HasTabs
        {
            get => _hasTabs;
            set
            {
                SetProperty(ref _hasTabs, value);
            }
        }

        public ObservableCollection<TabItemModel> TabsShowing
        {
            get => _tabsShowing;
        }

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

        public MedicamentosModel Medicamento
        {
            get => _medicamento;
            set
            {
                SetProperty(ref _medicamento, value);
            }
        }

        public MedicamentosFiltroModel Filtro
        {
            get => _filtro;
            set
            {
                SetProperty(ref _filtro, value);
            }
        }

        public string ValorPesquisa
        {
            get => _valorPesquisa;
            set
            {
                SetProperty(ref _valorPesquisa, value);
            }
        }

        public string ChavePesquisa
        {
            get => _chavePesquisa;
            set
            {
                SetProperty(ref _chavePesquisa, value);
            }
        }

        public Action? OnTabChanged { get; set; }
        public Action? OnInitialLoad { get; set; }

        public IAsyncRelayCommand CarregarMedicamentosCommand;
        public IAsyncRelayCommand<MedicamentosModel?> CadastrarMedicamentoCommand;
        public IAsyncRelayCommand<PesquisaMedicamento?> FiltrarMedicamentosCommand;

        public MedicamentosViewModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
            CarregarMedicamentosCommand = new AsyncRelayCommand(CarregarMedicamentosAsync);
            CadastrarMedicamentoCommand = new AsyncRelayCommand<MedicamentosModel?>(CadastrarMedicamentoAsync);
            FiltrarMedicamentosCommand = new AsyncRelayCommand<PesquisaMedicamento?>(BuscarMedicamentosFiltradosAsync);
        }

        #region metodos
        private async Task CarregarMedicamentosAsync()
        {
            try
            {
                if (Carregando)
                    return;

                Carregando = true;

                var medicamentos = await _http.GetFromJsonAsync<MedicamentoDTO[]>("api/medicamentos");

                Medicamentos.Clear();
                foreach (var m in medicamentos ?? [])
                    Medicamentos.Add(m);
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
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
            TabsShowing.First(t => t.Name == "Cadastrar").IsVisible = true;
            ActiveTab = TabsShowing.First(t => t.Name == "Cadastrar").Name;
        }

        private async Task CadastrarMedicamentoAsync(MedicamentosModel? med)
        {
            try
            {
                if (Cadastrando)
                    return;

                var dto = (MedicamentoDTO)med;

                Cadastrando = true;
                var response = await _http.PostAsJsonAsync("api/medicamentos", dto);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current!.MainPage!.DisplayAlert("Sucesso", "Medicamento cadastrado com sucesso!", "OK");
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    await Application.Current!.MainPage!.DisplayAlert(error!.Title, error!.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                Medicamento = new();
                Cadastrando = false;
            }
        }

        private async Task BuscarMedicamentosFiltradosAsync(PesquisaMedicamento? pesquisa)
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

                var url = $"api/medicamentos?{ChavePesquisa}={Uri.UnescapeDataString(ValorPesquisa)}";
                var medicamentos = await _http.GetFromJsonAsync<MedicamentoDTO[]>(url);

                Medicamentos.Clear();
                foreach (var m in medicamentos ?? [])
                    Medicamentos.Add(m);
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                Carregando = false;
            }
        }

        public static string DisplayNotaFiscal(MedicamentoDTO m) => string.IsNullOrEmpty(m.NotaFiscal) ? "-" : m.NotaFiscal;
        public static string DisplayDescricaoMedicamentos(MedicamentoDTO m) => string.IsNullOrEmpty(m.DescricaoMedicamentos) ? "-" : m.DescricaoMedicamentos;
        public static string DisplayValidade(MedicamentoDTO m) =>
            m.ValidadeMedicamento.HasValue ? m.ValidadeMedicamento.Value.ToString() : "-";
        public static string DisplayNFe(MedicamentoDTO p) => string.IsNullOrEmpty(p.NFe) ? "-" : p.NFe;
        public static string DisplayDescricaoDetalhada(MedicamentoDTO p) => string.IsNullOrEmpty(p.DescricaoDetalhada) ? "-" : p.DescricaoDetalhada;

        #endregion
    }
}

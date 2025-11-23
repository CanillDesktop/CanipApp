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
using System.Net.Http;
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
        private bool _deletando;

        private MedicamentosModel _medicamentoCadastro = new();
       
        private MedicamentosModel _medicamento = new();

      
        private MedicamentosFiltroModel _filtro = new();

        private string _chavePesquisa = string.Empty;
        private string _valorPesquisa = string.Empty;

        public ObservableCollection<MedicamentoLeituraDTO> Medicamentos { get; } = [];


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

        public bool Deletando
        {
            get => _deletando;
            set
            {
                SetProperty(ref _deletando, value);
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

        public MedicamentosModel MedicamentoCadastro
        {
            get => _medicamentoCadastro;
            set
            {
                SetProperty(ref _medicamentoCadastro, value);
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
        public IAsyncRelayCommand<PesquisaProduto?> FiltrarMedicamentosCommand;
        public IAsyncRelayCommand<MedicamentoLeituraDTO?> DeletarMedicamentoCommand;
     
        public IAsyncRelayCommand CarregarMedicamentosCommand { get; }
        public IAsyncRelayCommand<MedicamentosModel?> CadastrarMedicamentoCommand { get; }
        public IAsyncRelayCommand<PesquisaMedicamento?> FiltrarMedicamentosCommand { get; }
        public IAsyncRelayCommand SincronizarMedicamentoCommand { get; }
        public IAsyncRelayCommand<int> DeletarMedicamentoCommand { get; } 

        public MedicamentosViewModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");

            
            CarregarMedicamentosCommand = new AsyncRelayCommand(CarregarMedicamentosAsync);
            CadastrarMedicamentoCommand = new AsyncRelayCommand<MedicamentosModel?>(CadastrarMedicamentoAsync);
            FiltrarMedicamentosCommand = new AsyncRelayCommand<PesquisaProduto?>(BuscarMedicamentosFiltradosAsync);
            DeletarMedicamentoCommand = new AsyncRelayCommand<MedicamentoLeituraDTO?>(DeletarMedicamentoAsync);
        }

        #region metodos
        private async Task CarregarMedicamentosAsync()
        {
            try
            {
                if (Carregando)
                    return;

                Carregando = true;

                var medicamentos = await _http.GetFromJsonAsync<MedicamentoLeituraDTO[]>("api/medicamentos");
                
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

                var dto = (MedicamentoCadastroDTO)med;

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
                MedicamentoCadastro = new();
                Cadastrando = false;
            }
        }

        private async Task BuscarMedicamentosFiltradosAsync(PesquisaProduto? pesquisa)
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
                var medicamentos = await _http.GetFromJsonAsync<MedicamentoLeituraDTO[]>(url);

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

        private async Task DeletarMedicamentoAsync(MedicamentoLeituraDTO? m)
        {
            if (Carregando) return;
            Carregando = true;

            try
            {
                if (Deletando)
                    return;

                Deletando = true;

                var isExcluir = await Application.Current!.MainPage!.DisplayAlert("Confirmação de exclusão", $"Deseja realmente excluir o medicamento \"{m.NomeItem}\"?", "Sim", "Não");

                if (isExcluir)
                {
                    var result = await _http.DeleteAsync($"api/medicamentos/{m.IdItem}");
                    await CarregarMedicamentosAsync();
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                Deletando = false;
        }

        private async Task SincronizarMedicamentoAsyncFront()
        {
            await _http.PostAsync("api/Sync", null);
        }

        public static string DisplayDataEntregaRecente(MedicamentoLeituraDTO m) =>
            m.ItensEstoque.Length > 0 ?
            m.ItensEstoque?
            .Select(i => i.DataEntrega)?
            .OrderDescending()?
            .FirstOrDefault()
            .ToShortDateString() ?? "-"
            : "-";

        public static string DisplayDataValidadeRecente(MedicamentoLeituraDTO m) =>
            m.ItensEstoque?
            .Select(i => i.DataValidade)?
            .OrderDescending()?
            .FirstOrDefault()?
            .ToShortDateString() ?? "-";
        #endregion
    }

}
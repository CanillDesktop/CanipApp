using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Models; 
using Frontend.ViewModels.Interfaces;
using Shared.DTOs; 
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
        private bool _hasTabs = true;
        private readonly ObservableCollection<TabItemModel> _tabsShowing =
        [
            new TabItemModel("Medicamentos", true),
            new TabItemModel("Cadastrar",true)
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
            set => SetProperty(ref _carregando, value);
        }

        public bool Cadastrando
        {
            get => _cadastrando;
            set => SetProperty(ref _cadastrando, value);
        }

        public bool HasTabs
        {
            get => _hasTabs;
            set => SetProperty(ref _hasTabs, value);
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

        
        public MedicamentosModel MedicamentoParaCadastro
        {
            get => _medicamento;
            set => SetProperty(ref _medicamento, value);
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

        public Action? OnTabChanged { get; set; }
        public Action? OnInitialLoad { get; set; }

     
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
            FiltrarMedicamentosCommand = new AsyncRelayCommand<PesquisaMedicamento?>(BuscarMedicamentosFiltradosAsync);
            DeletarMedicamentoCommand = new AsyncRelayCommand<int>(DeletarMedicamentoAsync);
            SincronizarMedicamentoCommand = new AsyncRelayCommand(SincronizarMedicamentoAsyncFront);
        }

        #region Metodos
        private async Task CarregarMedicamentosAsync()
        {
            try
            {
                if (Carregando)
                    return;

                Carregando = true;

                
                var medicamentos = await _http.GetFromJsonAsync<MedicamentoDTO[]>("api/medicamentos");

                Medicamentos.Clear();
                foreach (var p in medicamentos ?? [])
                    Medicamentos.Add(p);
            }
            catch (Exception ex)
            {
                
                Debug.WriteLine($"Erro ao carregar medicamentos: {ex.Message}");
                
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
               
                if (Cadastrando || med is null)
                    return;

                
                var dto = (MedicamentoDTO)med;

                Cadastrando = true;
               
                var response = await _http.PostAsJsonAsync("api/medicamentos", dto);

                if (response.IsSuccessStatusCode)
                {
                    ;
                    Debug.WriteLine("Medicamento cadastrado com sucesso!");

                 
                    var itemSalvo = await response.Content.ReadFromJsonAsync<MedicamentoDTO>();
                    if (itemSalvo != null) Medicamentos.Add(itemSalvo);
                    ActiveTab = "Listar";
                    OnTabChanged?.Invoke();
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    Debug.WriteLine($"Erro: {error?.Title} - {error?.Message}");
                    
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao cadastrar medicamento: {ex.Message}");
               
            }
            finally
            {
                // Limpa o formulário
                MedicamentoParaCadastro = new();
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
                    Debug.WriteLine("Aviso: Escolha um campo e preencha o valor.");
                   
                    return;
                }


                var url = $"api/medicamentos?{ChavePesquisa}={Uri.EscapeDataString(ValorPesquisa)}";
                var medicamentos = await _http.GetFromJsonAsync<MedicamentoDTO[]>(url);

                Medicamentos.Clear();
                foreach (var p in medicamentos ?? [])
                    Medicamentos.Add(p);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao filtrar medicamentos: {ex.Message}");
                
            }
            finally
            {
                Carregando = false;
            }
        }

       
        private async Task DeletarMedicamentoAsync(int id)
        {
            if (Carregando) return;
            Carregando = true;

            try
            {
                var response = await _http.DeleteAsync($"api/medicamentos/{id}");
                response.EnsureSuccessStatusCode();
                var itemParaRemover = Medicamentos.FirstOrDefault(m => m.CodigoId == id);

                if (itemParaRemover != null)
                {
                    Medicamentos.Remove(itemParaRemover);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao deletar medicamento: {ex.Message}");
                
            }
            finally
            {
                Carregando = false;
            }
        }

        private async Task SincronizarMedicamentoAsyncFront()
        {
            await _http.PostAsync("api/Sync", null);
        }

        public static string DisplayNFe(MedicamentoDTO med) =>
            string.IsNullOrEmpty(med.NotaFiscal) ? "-" : med.NotaFiscal;

   
        public static string DisplayDescricaoDetalhada(MedicamentoDTO med) =>
            string.IsNullOrEmpty(med.DescricaoMedicamentos) ? "-" : med.DescricaoMedicamentos;

     
        public static string DisplayValidade(MedicamentoDTO med) =>
            med.ValidadeMedicamento.HasValue
            ? med.ValidadeMedicamento.Value.ToShortDateString()
            : "-";
        #endregion
    }
}
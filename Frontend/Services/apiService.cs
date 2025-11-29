using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shared.DTOs;
using Shared.DTOs.Medicamentos;
using Shared.DTOs.Produtos; // Importe seus DTOs

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // --- Métodos de Medicamentos ---

    public async Task<List<MedicamentoLeituraDTO>> GetMedicamentosAsync()
    {
        try
        {
            // Esta rota "api/Medicamentos" vem do seu Backend/Controllers/MedicamentosController.cs
            return await _httpClient.GetFromJsonAsync<List<MedicamentoLeituraDTO>>("api/Medicamentos");
        }
        catch (Exception ex)
        {
            // Trate o erro (ex: backend desligado)
            Console.WriteLine($"Erro ao buscar medicamentos: {ex.Message}");
            return new List<MedicamentoLeituraDTO>();
        }
    }

    public async Task<MedicamentoCadastroDTO> AddMedicamentoAsync(MedicamentoCadastroDTO dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Medicamentos", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MedicamentoCadastroDTO>();
    }

    // Adicione aqui UpdateMedicamentoAsync (PUT), DeleteMedicamentoAsync (DELETE), etc.

    // --- Métodos de Produtos ---

    public async Task<List<ProdutosLeituraDTO>> GetProdutosAsync()
    {
        // Esta rota "api/Produtos" vem do seu Backend/Controllers/ProdutosController.cs
        return await _httpClient.GetFromJsonAsync<List<ProdutosLeituraDTO>>("api/Produtos");
    }

    public async Task<ProdutosCadastroDTO> AddProdutoAsync(ProdutosCadastroDTO dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Produtos", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProdutosCadastroDTO>();
    }

    // Adicione aqui os métodos para Insumos, etc.
}
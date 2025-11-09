using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shared.DTOs; // Importe seus DTOs

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // --- Métodos de Medicamentos ---

    public async Task<List<MedicamentoDTO>> GetMedicamentosAsync()
    {
        try
        {
            // Esta rota "api/Medicamentos" vem do seu Backend/Controllers/MedicamentosController.cs
            return await _httpClient.GetFromJsonAsync<List<MedicamentoDTO>>("api/Medicamentos");
        }
        catch (Exception ex)
        {
            // Trate o erro (ex: backend desligado)
            Console.WriteLine($"Erro ao buscar medicamentos: {ex.Message}");
            return new List<MedicamentoDTO>();
        }
    }

    public async Task<MedicamentoDTO> AddMedicamentoAsync(MedicamentoDTO dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Medicamentos", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MedicamentoDTO>();
    }

    // Adicione aqui UpdateMedicamentoAsync (PUT), DeleteMedicamentoAsync (DELETE), etc.

    // --- Métodos de Produtos ---

    public async Task<List<ProdutosDTO>> GetProdutosAsync()
    {
        // Esta rota "api/Produtos" vem do seu Backend/Controllers/ProdutosController.cs
        return await _httpClient.GetFromJsonAsync<List<ProdutosDTO>>("api/Produtos");
    }

    public async Task<ProdutosDTO> AddProdutoAsync(ProdutosDTO dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Produtos", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProdutosDTO>();
    }

    // Adicione aqui os métodos para Insumos, etc.
}
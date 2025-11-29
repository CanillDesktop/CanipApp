using Backend.Exceptions;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Shared.DTOs;
using System.Globalization;
using System.Net.Http.Headers;
using Shared.Enums;
using Shared.DTOs.Medicamentos;
namespace Backend.Services
{
    public class MedicamentosService : IMedicamentosService
    {
        private readonly IMedicamentosRepository _repository;

        public MedicamentosService(IMedicamentosRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MedicamentoLeituraDTO>> BuscarTodosAsync() => (await _repository.GetAsync()).Select(p => (MedicamentoLeituraDTO)p);

        public async Task<MedicamentoLeituraDTO?> BuscarPorIdAsync(int id) => (await _repository.GetByIdAsync(id))!;

    
        public async Task<MedicamentoLeituraDTO?> CriarAsync(MedicamentoCadastroDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CodMedicamento)
                || string.IsNullOrWhiteSpace(dto.NomeComercial)
                || string.IsNullOrWhiteSpace(dto.Formula)
                || string.IsNullOrWhiteSpace(dto.DescricaoMedicamento)
                || !Enum.IsDefined(typeof(PrioridadeEnum), (int)dto.Prioridade)
                || !Enum.IsDefined(typeof(PublicoAlvoMedicamentoEnum), (int)dto.PublicoAlvo)
                || string.IsNullOrWhiteSpace(dto.Lote))
            {
                throw new ModelIncompletaException("Um ou mais campos obrigatórios não foram preenchidos");
            }

            return await _repository.CreateAsync(dto);
        }

        public async Task<MedicamentoLeituraDTO?> AtualizarAsync(MedicamentoCadastroDTO dto) => (await _repository.UpdateAsync(dto))!;

        public async Task<bool> DeletarAsync(int id) => await _repository.DeleteAsync(id);

        public async Task<IEnumerable<MedicamentoLeituraDTO>> BuscarTodosAsync(MedicamentosFiltroDTO filtro) => (await _repository.GetAsync(filtro)).Select(p => (MedicamentoLeituraDTO)p);


    


   
    }
}
using Backend.Exceptions;
using Backend.Models.Medicamentos;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Shared.DTOs.Insumos;
using Shared.DTOs.Medicamentos;
using Shared.Enums;
using Humanizer;
using Shared.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class InsumosService : IInsumosService
    {
        public readonly IInsumosRepository _repository;

        public InsumosService(IInsumosRepository insumosRepository)
        {
            _insumosRepository = insumosRepository;
        }

        public InsumosService(IInsumosRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<InsumosLeituraDTO>> BuscarTodosAsync() => (await _repository.GetAsync()).Select(p => (InsumosLeituraDTO)p);

        public async Task<InsumosLeituraDTO?> BuscarPorIdAsync(int id) => (await _repository.GetByIdAsync(id))!;

        public async Task<InsumosLeituraDTO?> CriarAsync(InsumosCadastroDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CodInsumo)
                || string.IsNullOrWhiteSpace(dto.DescricaoSimplificada)
                || !Enum.IsDefined(typeof(UnidadeInsumosEnum), (int)dto.Unidade)
                || string.IsNullOrWhiteSpace(dto.Lote))
        {
                throw new ModelIncompletaException("Um ou mais campos obrigatórios não foram preenchidos");
            }

            return await _repository.CreateAsync(dto);
        }

        public async Task<InsumosLeituraDTO?> AtualizarAsync(InsumosCadastroDTO dto) => (await _repository.UpdateAsync(dto))!;

        public async Task<bool> DeletarAsync(int id) => await _repository.DeleteAsync(id);

        public async Task<IEnumerable<InsumosLeituraDTO>> BuscarTodosAsync(InsumosFiltroDTO filtro) => (await _repository.GetAsync(filtro)).Select(p => (InsumosLeituraDTO)p);
    }
}
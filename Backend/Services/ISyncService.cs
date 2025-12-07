using Shared.DTOs;

public interface ISyncService
{
    Task SincronizarTabelasAsync();
    Task LimparRegistrosExcluidosAsync();

}
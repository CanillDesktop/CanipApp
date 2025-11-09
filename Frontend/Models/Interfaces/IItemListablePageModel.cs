namespace Frontend.Models.Interfaces
{
    public interface IItemListablePageModel<T> where T : notnull
    {
        // Cada item tem seu próprio estado expandido
        Dictionary<T, bool> ItensAbertos { get; set; }

        void Toggle(T codigo)
        {
            if (ItensAbertos.TryGetValue(codigo, out bool value))
                ItensAbertos[codigo] = ItensAbertos[codigo] = !value;
            else
                ItensAbertos[codigo] = true;
        }
    }
}

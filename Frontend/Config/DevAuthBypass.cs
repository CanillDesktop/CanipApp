namespace Frontend;

/// <summary>
/// Bypass da tela e do estado de login para desenvolvimento (UI e rotas).
/// APIs que exigem JWT no Backend continuam falhando até login real ou token válido.
/// </summary>
public static class DevAuthBypass
{
    /// <summary>
    /// Em Debug, retorna true por padrão (pule o login). Altere o return para false para retomar o fluxo normal.
    /// Em Release, sempre false.
    /// </summary>
    public static bool SkipLogin
    {
        get
        {
#if DEBUG
            return false;
#else
            return false;
#endif
        }
    }
}

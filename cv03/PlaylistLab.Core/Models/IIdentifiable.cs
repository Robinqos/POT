namespace PlaylistLab.Core.Models;

/// <summary>Položka s identifikátorom vhodným ako kľúč registra.</summary>
public interface IIdentifiable
{
    /// <summary>Neprázdny identifikátor položky.</summary>
    string Id { get; }
}

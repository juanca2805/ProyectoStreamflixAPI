using Xunit;

namespace StreamFlix.IntegrationTests;

/// <summary>
/// Agrupa todas las clases de test de integración bajo una sola "colección" de
/// xUnit, para que compartan una única instancia de StreamFlixApiFactory
/// (ICollectionFixture, a diferencia de IClassFixture, se crea UNA vez para
/// toda la colección, no una vez por clase).
///
/// Por qué importa: Program.cs corre Database.Migrate() al arrancar la app. Si
/// cada clase de test levantara su propia WebApplicationFactory (como con
/// IClassFixture), y xUnit las corre en paralelo por defecto, dos instancias de
/// la app podrían arrancar a la vez contra la misma base y aplicar la misma
/// migración nueva al mismo tiempo — contra una base ya migrada nunca se nota,
/// pero contra una base recién creada (como la del runner de CI) una de las dos
/// choca con "column ... already exists" y el WebApplicationFactory falla al
/// arrancar. Con una sola instancia compartida, Database.Migrate() corre una
/// única vez por toda la corrida de tests.
/// </summary>
[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<StreamFlixApiFactory>
{
    public const string Name = "Integration";
}

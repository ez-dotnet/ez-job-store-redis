# EZ.Job.Store.Redis

Store **Redis** para [EZ.Job.Core](https://github.com/ez-dotnet/ez-job-core).

## Performance

| Store  | Jobs | Workers | EZ.Job (ms) | Hangfire (ms) | Vezes mais rápido |
|--------|------|---------|-------------|---------------|-------------------|
| Redis  | 100  | 1       | 21.64       | 76.86         | 3.55×             |
| Redis  | 1000 | 4       | 117.15      | 381.56        | 3.26×             |

**Eficiência de memória:** EZ.Job aloca ~40% menos objetos por job comparado ao Hangfire, reduzindo pressão no GC.

## Instalação

```bash
dotnet add package EZ.Job.Store.Redis
```

## Uso

```csharp
builder.Services.AddEZJob()
    .AddRedisStore("localhost:6379");
```

## Projetos relacionados

- [EZ.DotNet](https://github.com/ez-dotnet)
- [EZ.Job.Core](https://github.com/ez-dotnet/ez-job-core)
- [EZ.Job.Recurring](https://github.com/ez-dotnet/ez-job-recurring)

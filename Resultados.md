# Resultados — Benchmark EZ.Job.Store.Redis

## Ambiente

| Item           | Valor                         |
|----------------|-------------------------------|
| Hardware       | Intel i7-12700K, 64GB DDR5   |
| SO             | Ubuntu 24.04                  |
| .NET           | 10.0                          |
| Driver         | NRedisStack 0.13.1 (StackExchange.Redis) |
| Redis          | 7.2 (Docker)                  |

## Resultados

| Jobs | Workers | EZ.Job (ms) | Hangfire (ms) | Vezes mais rápido |
|------|---------|-------------|---------------|-------------------|
| 100  | 1       | 21.64       | 76.86         | 3.55×             |
| 1000 | 4       | 117.15      | 381.56        | 3.26×             |

Ganho consistente de 3.3–3.6×.

## Eficiência de Memória

| Métrica                | EZ.Job | Hangfire |
|------------------------|--------|----------|
| Alocações por job      | ~2.4 KB| ~4.1 KB  |
| Objetos por job        | ~18    | ~31      |
| Pressão Gen 0/1/2      | Baixa  | Moderada |
